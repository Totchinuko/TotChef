using Microsoft.Extensions.Logging;
using Steamworks;
using tot.Services;
using tot_lib;

namespace Tot;

public class SteamWorks(KitchenFiles kitchenFiles, ILogger<SteamWorks> logger)
{
    private bool _initialized;
    private Timer? _callbackTimer;
    private int _pendingCalls;
    private bool _uploading;
    private UGCUpdateHandle_t? _uploadHandle;
    private CallResult<CreateItemResult_t>? _createItemResult;
    private string _action = string.Empty;
    private double _last = 0;

    public async Task<bool> InitializeSteam()
    {
        if (_initialized) return true;

        logger.LogInformation("Initializing Steam API");
        await File.WriteAllTextAsync(kitchenFiles.SteamAppIdPath.FullName, Constants.AppID.ToString());
        _initialized = SteamAPI.Init();
        if (!_initialized)
        {
            if(File.Exists(kitchenFiles.SteamAppIdPath.FullName))
                File.Delete(kitchenFiles.SteamAppIdPath.FullName);
            return false;
        }
        
        _callbackTimer = new Timer(OnTimerTick, null, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(50));
        return true;
    }
    
    public async Task ShutdownSteam()
    {
        logger.LogInformation("Shutting Down Steam API");
        if (_initialized && _pendingCalls <= 0)
        {
            if (_callbackTimer is not null)
            {
                _callbackTimer.Change(-1, -1);
                await _callbackTimer.DisposeAsync();
            }
            _callbackTimer = null;
            _createItemResult = null;
            _initialized = false;
            if (File.Exists(kitchenFiles.SteamAppIdPath.FullName))
            {
                File.Delete(kitchenFiles.SteamAppIdPath.FullName);
            }
            SteamAPI.Shutdown();
        }
    }

    private void OnTimerTick(object? state)
    {
        if (!_initialized) return;

        if (_pendingCalls > 0)
        {
            SteamAPI.RunCallbacks();
        }

        if (_uploading && _uploadHandle is { } handle)
        {
            SteamUGC.GetItemUpdateProgress(handle, out var processed, out var total);
            if (processed != 0 && total != 0)
            {
                var percent = processed / (double)total;
                if (Math.Abs(percent - _last) < 0.01) return;
                _last = percent;
                logger.LogInformation($"{_action}:{(_last * 100):N2}".Colorize(ConsoleColors.BLUE));
            }
        }
    }

    public async Task UploadMod(CancellationToken ct, bool skipContent)
    {
        if (!kitchenFiles.IsModPathValid())
            throw new Exception("Mod path is invalid");
        if (!kitchenFiles.ModPakFile.Exists)
            throw new FileNotFoundException("Mod Pak was not found");
        if (!await InitializeSteam())
            throw new Exception("Failed to connect to Steam");
        
        logger.LogInformation($"Starting upload for {kitchenFiles.ModName}");

        _action = "Initializing";
        var infos = await kitchenFiles.GetModInfos();
        if (!(await EnforceWorkshopId(infos)))
            throw new Exception("Failed to enforce the workshop ID");
        logger.LogInformation("Retrieved Published ID");
        UGCUpdateHandle_t? uGcUpdateHandleT = await MakeUpdateHandle(infos, skipContent);
        if (uGcUpdateHandleT.HasValue)
        {
            var value = uGcUpdateHandleT.GetValueOrDefault();
            if (!await SubmitUpload(value, infos.ChangeNote))
                throw new Exception("Upload failed");
        }

        await ShutdownSteam();
    }
    
    private async Task<bool> EnforceWorkshopId(ModinfoData infos)
    {
        string mainClient = infos.SteamWorkshopFileIds.MainClient;
        if (string.IsNullOrWhiteSpace(mainClient))
        {
            logger.LogInformation("No Workshop ID Found, requesting new ID");
            mainClient = await RequestNewWorkshopId();
            if (string.IsNullOrWhiteSpace(mainClient))
                return false;

            await SetWorkshopId(mainClient);
            logger.LogInformation($"Assigned new Workshop ID: {mainClient}");            
        }
        return true;
    }
    
    
    public async Task<string> RequestNewWorkshopId()
    {
        if (!_initialized)
            return string.Empty;
        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
        _pendingCalls++;
        SteamAPICall_t hApiCall = SteamUGC.CreateItem(new AppId_t(Constants.AppID), EWorkshopFileType.k_EWorkshopFileTypeFirst);
        if (_createItemResult == null)
        {
            _createItemResult = CallResult<CreateItemResult_t>.Create();
        }
        _createItemResult.Set(hApiCall, delegate(CreateItemResult_t result, bool failure)
        {
            _pendingCalls--;
            if (failure || result.m_eResult != EResult.k_EResultOK)
            {
                tcs.TrySetResult(string.Empty);
            }
            else
            {
                string result2 = result.m_nPublishedFileId.m_PublishedFileId.ToString();
                tcs.TrySetResult(result2);
            }
        });
        return await tcs.Task;
    }
    
    private async Task SetWorkshopId(string workshopId)
    {
        var infos = await kitchenFiles.GetModInfos();
        infos.SteamWorkshopFileIds.MainClient = workshopId;
        await kitchenFiles.SetModInfos(infos);
    }
    
    private async Task<UGCUpdateHandle_t?> MakeUpdateHandle(ModinfoData info, bool skipContent)
    {
        _action = "Preparing";
        ModTags modTags;
        try
        {
            modTags = await kitchenFiles.GetModTags();
        }
        catch
        {
            modTags = new();
        }
        
        if (!modTags.Tags.Contains("Enhanced"))
        {
            modTags.Tags.Add("Enhanced");
        }
        string cookedModPath = kitchenFiles.ModPakFile.FullName;
        if (!File.Exists(cookedModPath) && !skipContent)
            throw new FileNotFoundException("Mod pak was not found");
        
        string imagePath = kitchenFiles.ModPreviewImage.FullName;
        string mainClient = info.SteamWorkshopFileIds.MainClient;
        if (string.IsNullOrWhiteSpace(mainClient))
        {
            return null;
        }
        UGCUpdateHandle_t uGcUpdateHandleT = SteamUGC.StartItemUpdate(nPublishedFileID: new PublishedFileId_t(ulong.Parse(mainClient)), nConsumerAppId: new AppId_t(Constants.AppID));
        SteamUGC.SetItemTitle(uGcUpdateHandleT, string.IsNullOrWhiteSpace(info.Name) ? kitchenFiles.ModName : info.Name);
        SteamUGC.SetItemDescription(uGcUpdateHandleT, info.Description);
        SteamUGC.SetItemVisibility(uGcUpdateHandleT, (ERemoteStoragePublishedFileVisibility)info.SteamVisibility);
        SteamUGC.SetItemTags(uGcUpdateHandleT, modTags.Tags);
        if (!skipContent)
        {
            SteamUGC.SetItemContent(uGcUpdateHandleT, cookedModPath);
        }
        if (File.Exists(imagePath) && !skipContent)
        {
            SteamUGC.SetItemPreview(uGcUpdateHandleT, imagePath);
        }
        return uGcUpdateHandleT;
    }

    private Task<bool> SubmitUpload(UGCUpdateHandle_t handle, string changeNotes)
    {
        _uploading = true;
        _uploadHandle = handle;
        _pendingCalls++;
        _action = "Uploading";
        logger.LogInformation("Submitting...");
        TaskCompletionSource<bool> tcs = new();
        SteamAPICall_t hApiCall = SteamUGC.SubmitItemUpdate(handle, changeNotes);
        CallResult<SubmitItemUpdateResult_t>.Create().Set(hApiCall, delegate(SubmitItemUpdateResult_t callback, bool failure)
        {
            if (failure || callback.m_eResult != EResult.k_EResultOK)
            {
                _uploading = false;
                _pendingCalls--;
                logger.LogCritical("Steam upload failed");
                tcs.TrySetResult(false);
            }
            else
            {
                _uploading = false;
                logger.LogInformation("Mod uploaded successfully");
                tcs.TrySetResult(true);
            }
        });
        return tcs.Task;
    }
}