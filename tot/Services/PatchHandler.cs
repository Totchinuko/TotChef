using System.Text.Json;
using Tot;

namespace tot.Services;

public class PatchHandler
{
    public async Task<PatchNote> GetCurrentPatchNote()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var patchFile = Path.Combine(currentDir, Constants.PatchNoteFile);
        if (!File.Exists(patchFile)) return new PatchNote();
        var json = await File.ReadAllTextAsync(patchFile);
        var patchNote = JsonSerializer.Deserialize(json, PatchNoteJsonContext.Default.PatchNote);
        if (patchNote is null) return new PatchNote();
        return patchNote;
    }

    public async Task<bool> PatchNoteExists()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var patchFile = Path.Combine(currentDir, Constants.PatchNoteFile);
        if (!File.Exists(patchFile)) return false;
        var json = await File.ReadAllTextAsync(patchFile);
        var patchNote = JsonSerializer.Deserialize(json, PatchNoteJsonContext.Default.PatchNote);
        if (patchNote is null) return false;
        return patchNote.Additions.Count > 0 ||
               patchNote.Improvements.Count > 0 ||
               patchNote.Fixes.Count > 0;
    }

    public async Task SetCurrentPatchNote(PatchNote patchNote)
    {
        var json = JsonSerializer.Serialize(patchNote, PatchNoteJsonContext.Default.PatchNote);
        var currentDir = Directory.GetCurrentDirectory();
        var patchFile = Path.Combine(currentDir, Constants.PatchNoteFile);
        await File.WriteAllTextAsync(patchFile, json);
    }

    public void DeleteCurrentPatchNote()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var patchFile = Path.Combine(currentDir, Constants.PatchNoteFile);
        if (!File.Exists(patchFile)) return;
        File.Delete(patchFile);
    }
}