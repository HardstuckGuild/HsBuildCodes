using System.Diagnostics;
using System.Net;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class ProfessionSkillPallettes {
	public static readonly SkillPallette Guardian     = new();
	public static readonly SkillPallette Warrior      = new();
	public static readonly SkillPallette Engineer     = new();
	public static readonly SkillPallette Ranger       = new();
	public static readonly SkillPallette Thief        = new();
	public static readonly SkillPallette Elementalist = new();
	public static readonly SkillPallette Mesmer       = new();
	public static readonly SkillPallette Necromancer  = new();
	public static readonly SkillPallette Revenant     = new();

	public static SkillPallette ByProfession(Profession profession) => (profession) switch {
		Profession.Guardian     => Guardian    ,
		Profession.Warrior      => Warrior     ,
		Profession.Engineer     => Engineer    ,
		Profession.Ranger       => Ranger      ,
		Profession.Thief        => Thief       ,
		Profession.Elementalist => Elementalist,
		Profession.Mesmer       => Mesmer      ,
		Profession.Necromancer  => Necromancer ,
		Profession.Revenant     => Revenant    ,
		_ => throw new ArgumentOutOfRangeException(nameof(profession)),
	};

	/// <summary> This will only ever add new entries to all pallettes, never remove them. </summary>
	public static void ReloadAll(bool skipOnline = false)
	{
		//NOTE(Rennorb): This check is here so we dont reload Guardian twice. This would happen because the _FIRST member has the same value as Guardian, and therefore the value would get enumerated twice.
		// See remarks section of https://learn.microsoft.com/en-us/dotnet/api/system.enum.getvalues
		bool alreadyRanFIRSTReload = false;
		Task.WaitAll(Enum.GetValues<Profession>()
			.Where(profession => {
				if(profession == Profession._UNDEFINED) return false;
				if(profession == Profession._FIRST) {
					if(alreadyRanFIRSTReload) return false; 
					else alreadyRanFIRSTReload = true;
				} 
				return true;
			})
			.Select(profession => Reload(profession, skipOnline))
			.ToArray());
	}

	/// <summary> This will only ever add new entries, never remove them. </summary>
	public static async Task Reload(Profession profession, bool skipOnline = false)
	{
		var targetPallette = ByProfession(profession);
		if(targetPallette.PalletteToSkill.Count == 0) {
			targetPallette.PalletteToSkill.EnsureCapacity(10000);
			targetPallette.SkillToPallette.EnsureCapacity(10000);
		}

		bool loaded = false;
		if(!skipOnline) {
			try
			{
				//TODO(Rennorb): @performance
				var client = new Gw2Sharp.Gw2Client();
				var professionData = await client.WebApi.V2.Professions.GetAsync(Enum.GetName(profession)!);
				foreach(var (pallete, skill) in professionData.SkillsByPalette) {
					targetPallette.TryInsert((ushort)pallete, (SkillId)skill);
				}
				loaded = true;
			}
			catch(WebException ex)
			{
				Debug.WriteLine($"Could not fetch skill pallette for {profession}, will fall back to offline list.\n{ex}", "WARN");
			}
		}

		if(!loaded) ReloadFromOfflineFile(targetPallette, profession);

		targetPallette.TrimExcess();
	}

	internal static void ReloadFromOfflineFile(SkillPallette pallette, Profession profession)
	{
		//TODO(Rennorb): @performance
		foreach(var line in File.ReadLines($"offline/pallette-{profession}.csv").Skip(1))
		{
			var remaining = line.AsSpan();
			var palletteId = ushort.Parse(Util.Static.SliceAndAdvancePlus1(remaining.IndexOf(';'), ref remaining));

			pallette.TryInsert(palletteId, (SkillId)int.Parse(remaining));
		}
	}
}

public class SkillPallette {
	internal Dictionary<ushort, SkillId> PalletteToSkill = new();
	internal Dictionary<SkillId, ushort> SkillToPallette = new();

	public bool TryInsert(ushort palletteId, SkillId skillId)
	{
		var good1 = this.PalletteToSkill.TryAdd(palletteId, skillId);
		var good2 = this.SkillToPallette.TryAdd(skillId, palletteId);
		if(good1 != good2)
		{
			if(!good1) this.SkillToPallette.Remove(skillId);
			else       this.PalletteToSkill.Remove(palletteId);
		}
		return good1;
	}

	public void TrimExcess()
	{
		this.PalletteToSkill.TrimExcess();
		this.SkillToPallette.TrimExcess();
	}
}
