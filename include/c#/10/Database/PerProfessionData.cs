using System.Diagnostics;
using System.Net;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public class PerProfessionData {
	public static LazyLoadMode LazyLoadMode = LazyLoadMode.NONE;

	public static readonly PerProfessionData Guardian     = new();
	public static readonly PerProfessionData Warrior      = new();
	public static readonly PerProfessionData Engineer     = new();
	public static readonly PerProfessionData Ranger       = new();
	public static readonly PerProfessionData Thief        = new();
	public static readonly PerProfessionData Elementalist = new();
	public static readonly PerProfessionData Mesmer       = new();
	public static readonly PerProfessionData Necromancer  = new();
	public static readonly PerProfessionData Revenant     = new ();

	public static PerProfessionData ByProfession(Profession profession) => (profession) switch {
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

	DateTime _lastUpdate;

	/// <remarks> Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. </remarks>
	public Dictionary<ushort, SkillId> PalletteToSkill = new();
	/// <remarks> Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. </remarks>
	public Dictionary<SkillId, ushort> SkillToPallette = new();
	/// <remarks> Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. Indices are offset by 1. </remarks>
	public Dictionary<int, SpecializationId> IndexToId = new();
	/// <remarks> Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. Indices are offset by 1. </remarks>
	public Dictionary<SpecializationId, int> IdToIndex = new();

	public bool TryInsert(ushort palletteId, SkillId skillId)
	{
		var good1 = this.PalletteToSkill.TryAdd(palletteId, skillId);
		var good2 = this.SkillToPallette.TryAdd(skillId, palletteId);
		if(good1 != good2)
		{
			if(!good1) this.SkillToPallette.Remove(skillId);
			else this.PalletteToSkill.Remove(palletteId);
		}
		return good1;
	}

	public bool TryInsert(int professionSpecIndex, SpecializationId specId)
	{
		var good1 = this.IndexToId.TryAdd(professionSpecIndex, specId);
		var good2 = this.IdToIndex.TryAdd(specId, professionSpecIndex);
		if(good1 != good2)
		{
			if(!good1) this.IdToIndex.Remove(specId);
			else this.IndexToId.Remove(professionSpecIndex);
		}
		return good1;
	}

	public void Assign(ushort palletteId, SkillId skillId)
	{
		this.PalletteToSkill[palletteId] = skillId;
		this.SkillToPallette[skillId] = palletteId;
	}

	public void Assign(int professionSpecIndex, SpecializationId specId)
	{
		this.IndexToId[professionSpecIndex] = specId;
		this.IdToIndex[specId] = professionSpecIndex;
	}

	public void TrimExcess()
	{
		this.PalletteToSkill.TrimExcess();
		this.SkillToPallette.TrimExcess();
		this.IndexToId.TrimExcess();
		this.IdToIndex.TrimExcess();
	}

	internal void ReloadFromOfflineFiles(Profession profession)
	{
		//TODO(Rennorb): @performance
		foreach(var line in File.ReadLines($"offline/pallettes/{profession}.csv").Skip(1))
		{
			var remaining = line.AsSpan();
			var palletteId = ushort.Parse(Util.Static.SliceAndAdvancePlus1(remaining.IndexOf(';'), ref remaining));

			this.TryInsert(palletteId, (SkillId)int.Parse(remaining));
		}

		//TODO(Rennorb): @performance
		foreach(var line in File.ReadLines($"offline/specializations/{profession}.csv").Skip(1))
		{
			var remaining = line.AsSpan();
			var index = ushort.Parse(Util.Static.SliceAndAdvancePlus1(remaining.IndexOf(';'), ref remaining));

			this.TryInsert(index + 1, (SpecializationId)int.Parse(remaining));
		}
	}

	/// <remarks> This will only ever add new entries, never remove them. </remarks>
	public static void ReloadAll(bool skipOnline = false)
	{
		Task.WaitAll(Enum.GetValues<Profession>()
			.Where(profession => {
				if(profession == Profession._UNDEFINED) return false;
				return true;
			})
			.Select(profession => Reload(profession, skipOnline))
			.ToArray());
	}

	/// <remarks> This will only ever add new entries, never remove them. </remarks>
	public static async Task Reload(Profession profession, bool skipOnline = false)
	{
		var targetData = ByProfession(profession);

		if(DateTime.Now - targetData._lastUpdate < TimeSpan.FromMinutes(5)) return;

		if(targetData.PalletteToSkill.Count == 0)
		{
			targetData.PalletteToSkill.EnsureCapacity(10000);
			targetData.SkillToPallette.EnsureCapacity(10000);
			targetData.IdToIndex.EnsureCapacity(10000);
			targetData.IndexToId.EnsureCapacity(10000);

			targetData.TryInsert(0, SkillId._UNDEFINED);
			targetData.TryInsert(0, SpecializationId._UNDEFINED);
		}

		bool loaded = false;
		if(!skipOnline)
		{
			try
			{

				var professionData = await APICache.Get<OfficialAPI.Profession>($"/professions/{Enum.GetName(profession)}", "2019-12-19T00:00:00.000Z");
				foreach(var (pallete, skill) in professionData.SkillsByPalette!)
				{
					targetData.Assign((ushort)pallete, (SkillId)skill);
				}
				int i = 1;
				foreach(var specId in professionData.Specializations)
				{
					targetData.Assign(i++, (SpecializationId)specId);
				}
				loaded = true;
			}
			catch(WebException ex)
			{
				Debug.WriteLine($"Could not fetch skill pallette for {profession}, will fall back to offline list.\n{ex}", "WARN");
			}
		}

		if(!loaded) {
			targetData.ReloadFromOfflineFiles(profession);
		}

		Overrides.LoadAdditionalPerProfessionData(profession, targetData);

		targetData.TrimExcess();

		targetData._lastUpdate = DateTime.Now;
	}
}

public enum LazyLoadMode {
	NONE = default,
	OFFLINE_ONLY,
	FULL,
}
