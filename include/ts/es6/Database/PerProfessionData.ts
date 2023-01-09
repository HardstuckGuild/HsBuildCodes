import APICache from "../OfficialAPI/APICache";
import { Profession } from "../Structures";
import LazyLoadMode from "./LazyLoadMode";
import Overrides from "./Overrides";
import SkillId from "./SkillIds";
import SpecializationId from "./SpecializationIds";

type Store<A extends string | number | symbol, B> = {
	[index in A]: B;
};
class PerProfessionData {
	public static LazyLoadMode : LazyLoadMode = LazyLoadMode.NONE;

	public static Guardian     : PerProfessionData = new PerProfessionData();
	public static Warrior      : PerProfessionData = new PerProfessionData();
	public static Engineer     : PerProfessionData = new PerProfessionData();
	public static Ranger       : PerProfessionData = new PerProfessionData();
	public static Thief        : PerProfessionData = new PerProfessionData();
	public static Elementalist : PerProfessionData = new PerProfessionData();
	public static Mesmer       : PerProfessionData = new PerProfessionData();
	public static Necromancer  : PerProfessionData = new PerProfessionData();
	public static Revenant     : PerProfessionData = new PerProfessionData();

	public static ByProfession(profession : Profession) : PerProfessionData
	{
		switch (profession) {
			case Profession.Guardian    : return PerProfessionData.Guardian    ;
			case Profession.Warrior     : return PerProfessionData.Warrior     ;
			case Profession.Engineer    : return PerProfessionData.Engineer    ;
			case Profession.Ranger      : return PerProfessionData.Ranger      ;
			case Profession.Thief       : return PerProfessionData.Thief       ;
			case Profession.Elementalist: return PerProfessionData.Elementalist;
			case Profession.Mesmer      : return PerProfessionData.Mesmer      ;
			case Profession.Necromancer : return PerProfessionData.Necromancer ;
			case Profession.Revenant    : return PerProfessionData.Revenant    ;
			default: throw new Error("invalid profession");
		};
	}

	private _lastUpdate : Date;

	private constructor() {
		this._lastUpdate = new Date("1970-01-01");
	}

	/** @remarks Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. */
	public PalletteToSkill : Store<number, SkillId> = {};
	/** @remarks Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. */
	public SkillToPallette : Store<SkillId, number> = {} as any;
	/** @remarks Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. Indices are offset by 1. */
	public IndexToId : Store<number, SpecializationId> = {};
	/** @remarks Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. Indices are offset by 1. */
	public IdToIndex : Store<SpecializationId, number> = {} as any;

	public TryInsertSkill(palletteId : number, skillId : SkillId) : boolean
	{
		const good1 = this.PalletteToSkill[palletteId] === undefined;
		const good2 = this.SkillToPallette[skillId] === undefined;
		if(good1 && good2)
		{
			this.PalletteToSkill[palletteId] = skillId;
			this.SkillToPallette[skillId] = palletteId;
			return true;
		}
		return false;
	}

	public TryInsertSpec(professionSpecIndex : number, specId : number) : boolean
	{
		const good1 = this.IndexToId[professionSpecIndex] === undefined;
		const good2 = this.IdToIndex[specId] === undefined;
		if(good1 && good2)
		{
			this.IndexToId[professionSpecIndex] = specId;
			this.IdToIndex[specId] = professionSpecIndex;
			return true;
		}
		return false;
	}

	public AssignSkill(palletteId : number, skillId : SkillId) : void
	{
		this.PalletteToSkill[palletteId] = skillId;
		this.SkillToPallette[skillId] = palletteId;
	}

	public AssignSpec(professionSpecIndex : number, specId : number) : void
	{
		this.IndexToId[professionSpecIndex] = specId;
		this.IdToIndex[specId] = professionSpecIndex;
	}

	//NOTE(Rennorb): cant trim arrays in js

	// ReloadFromOfflineFiles(profession : Profession) : void
	// {
	// 	let file = fopen("offline/pallettes/{profession.name}.csv", 'r');
	// 	while((line = fgets(file)) !== false)
	// 	{
	// 		list(palletteId_s, skillId_s) = explode(';', line, 2);
	// 		this.TryInsertSkill(intval(palletteId_s), intval(skillId_s));
	// 	}

	// 	file = fopen("offline/specializations/{profession.name}.csv", 'r', use_include_path: true);
	// 	while((line = fgets(file)) !== false)
	// 	{
	// 		list(index_s, specialization_s) = explode(';', line, 2);
	// 		this.TryInsertSpec(intval(index_s) + 1, intval(specialization_s));
	// 	}
	// }

	/** @remarks This will only ever add new entries, never remove them. */
	public static ReloadAll(/*skipOnline : boolean = false*/) : Promise<void[]>
	{
		return Promise.all(Object.values(Profession)
			.filter(p => p !== Profession._UNDEFINED && typeof p !== 'string')
			.map(p => PerProfessionData.Reload(p as Profession/*, skipOnline*/)));
	}

	/** @remarks This will only ever add new entries, never remove them. */
	public static async Reload(profession : Profession/*, skipOnline : boolean = false*/) : Promise<void>
	{
		const targetData = PerProfessionData.ByProfession(profession);

		if((new Date).getTime() - targetData._lastUpdate.getTime() < 5 * 60) return;

		if(targetData.PalletteToSkill[0] === undefined)
		{
			//NOTE(Rennorb): js doesn't allow preallocation

			targetData.TryInsertSkill(0, SkillId._UNDEFINED);
			targetData.TryInsertSpec(0, SpecializationId._UNDEFINED);
		}

		let loaded = false;
		// if(!skipOnline)
		// {
			try
			{
				const professionData = await APICache.Get("/professions/"+Profession[profession], '2019-12-19T00:00:00.000Z');
				for(const [palette, skill] of professionData.skills_by_palette)
				{
					targetData.AssignSkill(palette, skill);
				}
				let i = 1;
				for(const specId of professionData.specializations)
				{
					targetData.AssignSpec(i++, specId);
				}
				loaded = true;
			}
			catch(ex : any)
			{
				//console.warn("Could not fetch skill pallette for $professionName, will fall back to offline list.", ex);
				console.error(`Could not fetch skill pallette for ${Profession[profession]}.`, ex);
			}
		// }

		// if(!loaded) {
		// 	targetData.ReloadFromOfflineFiles(profession);
		// }

		Overrides.LoadAdditionalPerProfessionData(profession, targetData);

		//NOTE(Rennorb): no trimming either
		//targetData.TrimExcess();

		targetData._lastUpdate = new Date();
	}
}
export default PerProfessionData;
