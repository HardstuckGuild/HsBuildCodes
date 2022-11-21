import APICache from "../OfficialAPI/APICache";
import { Profession } from "../Structures";
import LazyLoadMode from "./LazyLoadMode";
import SkillId from "./SkillIds";
import SpecializationId from "./SpecializationIds";

class PerProfessionData {
	public static LazyLoadMode : LazyLoadMode = LazyLoadMode.NONE;

	public static Guardian     : PerProfessionData;
	public static Warrior      : PerProfessionData;
	public static Engineer     : PerProfessionData;
	public static Ranger       : PerProfessionData;
	public static Thief        : PerProfessionData;
	public static Elementalist : PerProfessionData;
	public static Mesmer       : PerProfessionData;
	public static Necromancer  : PerProfessionData;
	public static Revenant     : PerProfessionData;

	public static __construct_static() {
		PerProfessionData.Guardian     = new PerProfessionData();
		PerProfessionData.Warrior      = new PerProfessionData();
		PerProfessionData.Engineer     = new PerProfessionData();
		PerProfessionData.Ranger       = new PerProfessionData();
		PerProfessionData.Thief        = new PerProfessionData();
		PerProfessionData.Elementalist = new PerProfessionData();
		PerProfessionData.Mesmer       = new PerProfessionData();
		PerProfessionData.Necromancer  = new PerProfessionData();
		PerProfessionData.Revenant     = new PerProfessionData();
	}

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
	public PalletteToSkill : Array<SkillId> = [];
	/** @remarks Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. */
	public SkillToPallette : Array<number> = [];
	/** @remarks Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. Indices are offset by 1. */
	public IndexToId : Array<number> = [];
	/** @remarks Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. Indices are offset by 1. */
	public IdToIndex : Array<number> = [];

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
	public static ReloadAll(/*skipOnline : boolean = false*/) : void
	{
		//TODO(Rennorb): make parallel
		for(const profession of Object.values(Profession)) {
			if(profession === Profession._UNDEFINED) continue;

			PerProfessionData.Reload(profession as Profession/*, skipOnline*/);
		}
	}

	/** @remarks This will only ever add new entries, never remove them. */
	public static Reload(profession : Profession/*, skipOnline : boolean = false*/) : void
	{
		const targetData = PerProfessionData.ByProfession(profession);

		if((new Date).getTime() - targetData._lastUpdate.getTime() < 5 * 60) return;

		if(targetData.PalletteToSkill.length === 0)
		{
			//NOTE(Rennorb): php doesn't allow preallocation

			targetData.TryInsertSkill(0, SkillId._UNDEFINED);
			targetData.TryInsertSpec(0, SpecializationId._UNDEFINED);
		}

		let loaded = false;
		// if(!skipOnline)
		// {
			try
			{
				const professionData = APICache.Get("/professions/"+Profession[profession], '2019-12-19T00:00:00.000Z');
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

		//NOTE(Rennorb): no trimming either
		//targetData.TrimExcess();

		targetData._lastUpdate = new Date();
	}
}
export default PerProfessionData;
