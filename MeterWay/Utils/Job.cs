using Dalamud.Plugin.Services;

namespace MeterWay.Utils;

public enum JobIds : int
{
    UNK = -1,
    ADV, GLA, PGL, MRD, LNC, ARC, CNJ, THM,
    CRP, BSM, ARM, GSM, LTW, WVR, ALC, CUL,
    MIN, BTN, FSH, PLD, MNK, WAR, DRG, BRD,
    WHM, BLM, ACN, SMN, SCH, ROG, NIN, MCH,
    DRK, AST, SAM, RDM, BLU, GNB, DNC, RPR, SGE
}

public class Job(uint rawId)
{
    public JobIds Id = rawId <= 40 ? (JobIds)rawId : JobIds.UNK;
    public nint Icon() => Id == JobIds.UNK ? 0 : Dalamud.Textures.GetIcon((uint)Id + 62000u, ITextureProvider.IconFlags.None)?.ImGuiHandle ?? 0;
}