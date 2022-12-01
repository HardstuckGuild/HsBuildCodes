import ItemId from "../Database/ItemIds";
import SkillId from "../Database/SkillIds";
import SpecializationId from "../Database/SpecializationIds";
import { IsTwoHanded } from "../Database/Static";
import StatId from "../Database/StatIds";
import TraitId from "../Database/TraitIds";
import { BuildCode, Profession, Specialization, TraitLineChoice, TraitSlot, WeaponSet, WeaponType } from "../Structures";
import { Assert } from "../Util/Static";
import { ICache, DefaultCacheImpl } from "./CacheImpl";

class APICache {
	public static CacheImpl : ICache;
	
	public static Get(path : string, schemaVersion : string = 'latest') : Promise<any>
	{
		if(APICache.CacheImpl === undefined)
			APICache.CacheImpl = new DefaultCacheImpl();
		return APICache.CacheImpl.Get(path, schemaVersion);
	}

	public static async ResolveWeaponType(itemId : ItemId) : Promise<WeaponType>
	{
		const itemData = await APICache.Get("/items/"+itemId);
		Assert(itemData.type === "Weapon", "Item is not a weapon:", itemData);

		return WeaponType[itemData.details.type as keyof typeof WeaponType];
	}

	/** 
	 * @return int StatId._UNDEFINED if the item does not have stats.
	 */
	public static async ResolveStatId(itemId : ItemId) : Promise<StatId>
	{
		const itemData = await APICache.Get("/items/"+itemId);
		return itemData.details.infix_upgrade?.id ?? StatId._UNDEFINED;
	}

	public static async ResolvePosition(traitId : TraitId) : Promise<TraitLineChoice>
	{
		if(!traitId) return TraitLineChoice.NONE;

		const traitData = await APICache.Get("/traits/"+traitId);
		return (traitData.order + 1) as TraitLineChoice;
	}

	public static async ResolveWeaponSkill(code : BuildCode, effectiveWeapons : WeaponSet, skillIndex : number) : Promise<SkillId>
	{
		let weapon : any = null;
		if(skillIndex < 3)
		{
			if(effectiveWeapons.MainHand === WeaponType._UNDEFINED) return SkillId._UNDEFINED;

			//NOTE(Rennorb): this isnt outside of the if to allow early bail if the guard condition isnt met.
			const professionData = await APICache.Get("/professions/"+Profession[code.Profession]);
			
			weapon = professionData.weapons[WeaponType[effectiveWeapons.MainHand]];
		}
		else
		{
			if(effectiveWeapons.OffHand === WeaponType._UNDEFINED
				&& (effectiveWeapons.MainHand === WeaponType._UNDEFINED || !IsTwoHanded(effectiveWeapons.MainHand)))
				return SkillId._UNDEFINED;

			//NOTE(Rennorb): this isnt outside of the if to allow early bail if the guard condition isnt met.
			const professionData = await APICache.Get("/professions/"+Profession[code.Profession]);
			if(effectiveWeapons.OffHand !== WeaponType._UNDEFINED)
				weapon = professionData.weapons[WeaponType[effectiveWeapons.OffHand]];
			else
				weapon = professionData.weapons[WeaponType[effectiveWeapons.MainHand]];
		}

		for(const skill of weapon.skills)
		{
			if(skill.slot === "Weapon_"+(skillIndex + 1))
				return skill.id;
		}
		
		return SkillId._UNDEFINED;
	}

	/** @return TraitId._UNDEFINED If spec is empty */
	public static async ResolveTrait(spec : Specialization, traitSlot : TraitSlot) : Promise<TraitId>
	{
		if(spec.SpecializationId === SpecializationId._UNDEFINED) return TraitId._UNDEFINED;
		const traitPos = spec.Choices[traitSlot];
		if(traitPos === TraitLineChoice.NONE) return TraitId._UNDEFINED;

		const allSpecializationData = await APICache.Get('/specializations?ids=all');

		for(const specialization of allSpecializationData)
		{
			if(specialization.id !== spec.SpecializationId) continue;

			return specialization.major_traits[traitSlot * 3 + traitPos - 1];
		}

		return TraitId._UNDEFINED;
	}
}
export default APICache;
