<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

use Hardstuck\GuildWars2\BuildCodes\V2\Util\TraitLineChoices;

class APILoader {
	use Util\_Static;

	/** Produces a list of token scopes that are missing. */
	public static function ValidateScopes(string $token) : array
	{
		$tokenInfo = API::RequestJson("/tokeninfo", $token);

		$required = [ "account", "characters", "builds"  ];
		return array_values(array_diff($required, $tokenInfo->permissions));
	}

	//NOTE(Rennorb): Removed Load from current character because php is not run on clients

	/** 
	 * @remarks This method assumes the scopes account, character and build are available, but does not explicitly test for them.
	 * @throws \Exception If the character can't be found.
	 * @throws \Exception If scopes are missing.
	 * @throws \Exception If the token is not valid.
	 */
	public static function LoadBuildCode(string $authToken, string $characterName, int $targetGameMode, bool $aquatic = false) : BuildCode
	{
		$code = new BuildCode();
		$code->Version = CURRENT_VERSION;
		$code->Kind    = $targetGameMode;

		$playerData = API::RequestJson("/characters/$characterName", $authToken);
		
		$code->Profession = Profession::GetValue($playerData->profession);

		$activeBuild = $playerData->build_tabs[$playerData->active_build_tab - 1]->build;
		for($i = 0; $i < 3; $i++) {
			$spec = $activeBuild->specializations[$i];
			if($spec->id === null) continue;

			$choices = new TraitLineChoices();
			$choices->Adept       = APICache::ResolvePosition($spec->traits[0]);
			$choices->Master      = APICache::ResolvePosition($spec->traits[1]);
			$choices->Grandmaster = APICache::ResolvePosition($spec->traits[2]);
			$code->Specializations[$i] = new Specialization($spec->id, $choices);
		}

		//$activeEquipment = $playerData->equipment_tabs[$playerData->active_equipment_tab - 1];
		$activeEquipment = $playerData; // #15
		if($targetGameMode !== Kind::PvP)
		{
			$runeId = null;

			$SetArmorData = function(int $equipSlot, $item) use($code, &$runeId) : void  {
				$code->EquipmentAttributes[$equipSlot] = APILoader::ResolveStatId($item);
				$code->Infusions          [$equipSlot] = property_exists($item, "infusions") ? $item->infusions[0] : ItemId::_UNDEFINED;
				if(property_exists($item, "upgrades")) {
					if($runeId === null) $runeId = $item->upgrades[0];
					else if($runeId !== $item->upgrades[0]) $runeId = ItemId::_UNDEFINED;
				}
			};

			foreach($activeEquipment->equipment as $item) {
				//NOTE(Rennorb): #15 can be removed once the api bug is fixed as the data in templates already only shows the equipped stuff
				if($item->location === "Armory" || $item->location === "LegendaryArmory")
					continue;

				$hasInfusions = property_exists($item, "infusions");
				$hasUpgrades  = property_exists($item, "upgrades");

				switch($item->slot)
				{
					case "Helm"       : if( $aquatic) break; $SetArmorData(0, $item); break;
					case "HelmAquatic": if(!$aquatic) break; $SetArmorData(0, $item); break;
					case "Shoulders"  :                      $SetArmorData(1, $item); break;
					case "Coat"       :                      $SetArmorData(2, $item); break;
					case "Gloves"     :                      $SetArmorData(3, $item); break;
					case "Leggings"   :                      $SetArmorData(4, $item); break;
					case "Boots"      :                      $SetArmorData(5, $item); break;
						
					case "Backpack":
						$code->EquipmentAttributes->BackItem = APILoader::ResolveStatId($item);
						if($hasInfusions) {
							$code->Infusions->BackItem_1 = $item->infusions[0];
							if(count($item->infusions) > 1)
								$code->Infusions->BackItem_2 = $item->infusions[1];
						}
						break;

					case "Accessory1":
						$code->EquipmentAttributes->Accessory1 = APILoader::ResolveStatId($item);
						$code->Infusions          ->Accessory1 = $hasInfusions ? $item->infusions[0] : ItemId::_UNDEFINED;
						break;

					case "Accessory2":
						$code->EquipmentAttributes->Accessory2 = APILoader::ResolveStatId($item);
						$code->Infusions          ->Accessory2 = $hasInfusions ? $item->infusions[0] : ItemId::_UNDEFINED;
						break;

					case "Ring1":
						$code->EquipmentAttributes->Ring1 = APILoader::ResolveStatId($item);
						if($hasInfusions) {
							$code->Infusions->Ring1_1 = $item->infusions[0];
							if(count($item->infusions) > 1) {
								$code->Infusions->Ring1_2 = $item->infusions[1];
								if(count($item->infusions) > 2)
									$code->Infusions->Ring1_3 = $item->infusions[2];
							}
						}
						break;
						
					case "Ring2":
						$code->EquipmentAttributes->Ring2 = APILoader::ResolveStatId($item);
						if($hasInfusions) {
							$code->Infusions->Ring2_1 = $item->infusions[0];
							if(count($item->infusions) > 1) {
								$code->Infusions->Ring2_2 = $item->infusions[1];
								if(count($item->infusions) > 2)
									$code->Infusions->Ring2_3 = $item->infusions[2];
							}
						}
						break;
						
					case "WeaponA1":
						if($aquatic) break;
						$code->EquipmentAttributes->WeaponSet1MainHand = APILoader::ResolveStatId($item);
						if($hasInfusions) {
							$code->Infusions->WeaponSet1_1 = $item->infusions[0];
							if(count($item->infusions) > 1)
								$code->Infusions->WeaponSet1_2 = $item->infusions[1];
						}
						$code->WeaponSet1->MainHand = APICache::ResolveWeaponType($item->id);
						if($hasUpgrades) {
							$code->WeaponSet1->Sigil1 = $item->upgrades[0];
							if(count($item->upgrades) > 1)
								$code->WeaponSet1->Sigil2 = $item->upgrades[1];
						}
						break;

					case "WeaponAquaticA":
						if(!$aquatic) break;
						$code->EquipmentAttributes->WeaponSet1MainHand = APILoader::ResolveStatId($item);
						if($hasInfusions) {
							$code->Infusions->WeaponSet1_1 = $item->infusions[0];
							if(count($item->infusions) > 1)
								$code->Infusions->WeaponSet1_2 = $item->infusions[1];
						}
						$code->WeaponSet1->MainHand = APICache::ResolveWeaponType($item->id);
						if($hasUpgrades) {
							$code->WeaponSet1->Sigil1 = $item->upgrades[0];
							if(count($item->upgrades) > 1)
								$code->WeaponSet1->Sigil2 = $item->upgrades[1];
						}
						break;

					case "WeaponA2":
						if($aquatic) break;
						$code->EquipmentAttributes->WeaponSet1OffHand = APILoader::ResolveStatId($item);
						$code->Infusions->WeaponSet1_2 = $hasInfusions ? $item->infusions[0] : ItemId::_UNDEFINED; //NOTE(Rennorb): this assues that buidls with twohanded main weapons dont contain an 'empty' weapon with no upgrades
						$code->WeaponSet1->OffHand = APICache::ResolveWeaponType($item->id);
						$code->WeaponSet1->Sigil2 =  $hasUpgrades ? $item->upgrades[0] : ItemId::_UNDEFINED; //NOTE(Rennorb): this assues that buidls with twohanded main weapons dont contain an 'empty' weapon with no upgrades
						break;

					case "WeaponB1":
						if($aquatic) break;
						$code->EquipmentAttributes->WeaponSet2MainHand = APILoader::ResolveStatId($item);
						if($hasInfusions) {
							$code->Infusions->WeaponSet2_1 = $item->infusions[0];
							if(count($item->infusions) > 1)
								$code->Infusions->WeaponSet2_2 = $item->infusions[1];
						}
						$code->WeaponSet2->MainHand = APICache::ResolveWeaponType($item->id);
						if($hasUpgrades) {
							$code->WeaponSet2->Sigil1 = $item->upgrades[0];
							if(IsTwoHanded($code->WeaponSet2->MainHand) && count($item->upgrades) > 1)
								$code->WeaponSet2->Sigil2 = $item->upgrades[1];
						}
						break;

					case "WeaponAquaticB":
						if(!$aquatic) break;
						$code->EquipmentAttributes->WeaponSet2MainHand = APILoader::ResolveStatId($item);
						if($hasInfusions) {
							$code->Infusions->WeaponSet2_1 = $item->infusions[0];
							if(count($item->infusions) > 1)
								$code->Infusions->WeaponSet2_2 = $item->infusions[1];
						}
						$code->WeaponSet2->MainHand = APICache::ResolveWeaponType($item->id);
						if($hasUpgrades) {
							$code->WeaponSet2->Sigil1 = $item->upgrades[0];
							if(count($item->upgrades) > 1)
								$code->WeaponSet2->Sigil2 = $item->upgrades[1];
						}
						break;

					case "WeaponB2":
						if($aquatic) break;
						$code->EquipmentAttributes->WeaponSet2OffHand = APILoader::ResolveStatId($item);
						$code->Infusions->WeaponSet2_2 = $hasInfusions ? $item->infusions[0] : ItemId::_UNDEFINED;
						$code->WeaponSet2->OffHand = APICache::ResolveWeaponType($item->id);
						$code->WeaponSet2->Sigil2 = $hasUpgrades ? $item->upgrades[0] : ItemId::_UNDEFINED;
						break;

					case "Amulet":
						if($aquatic) break;
						$code->EquipmentAttributes->Amulet = APILoader::ResolveStatId($item);
						$code->Infusions          ->Amulet = $hasInfusions ? $item->infusions[0] : ItemId::_UNDEFINED;
						break;
				}
			}

			if($runeId !== 0) $code->Rune = $runeId ?? ItemId::_UNDEFINED;
		}
		else // WvW, PvE
		{
			//$pvpEquip = $activeEquipment->equipment_pvp;
			$pvpEquip = $playerData->equipment_tabs[$playerData->active_equipment_tab - 1]->equipment_pvp; // #15

			foreach($activeEquipment->equipment as $item) {
				//NOTE(Rennorb): #15 can be removed once the api bug is fixed as the data in templates already only shows the equipped stuff
				if($item->location === "Armory" || $item->location === "LegendaryArmory")
					continue;

				switch($item->slot) {
					case "WeaponA1"      : if(!$aquatic) $code->WeaponSet1->MainHand = APICache::ResolveWeaponType($item->id); break;
					case "WeaponAquaticA": if( $aquatic) $code->WeaponSet1->MainHand = APICache::ResolveWeaponType($item->id); break;
					case "WeaponA2"      : if(!$aquatic) $code->WeaponSet1->OffHand  = APICache::ResolveWeaponType($item->id); break;
					case "WeaponB1"      : if(!$aquatic) $code->WeaponSet2->MainHand = APICache::ResolveWeaponType($item->id); break;
					case "WeaponAquaticB": if( $aquatic) $code->WeaponSet2->MainHand = APICache::ResolveWeaponType($item->id); break;
					case "WeaponB2"      : if(!$aquatic) $code->WeaponSet2->OffHand  = APICache::ResolveWeaponType($item->id); break;
				}
			}

			$code->EquipmentAttributes->Amulet = $pvpEquip->amulet ?? 0;
			$code->Rune = $pvpEquip->rune ?? 0;
			$code->WeaponSet1->Sigil1 = $pvpEquip->sigils[0] ?? 0;
			$code->WeaponSet1->Sigil2 = $pvpEquip->sigils[1] ?? 0;
			$code->WeaponSet2->Sigil1 = $pvpEquip->sigils[2] ?? 0;
			$code->WeaponSet2->Sigil2 = $pvpEquip->sigils[3] ?? 0;
		}

		// swap weapon set so the first set always has the waepons if there are any.
		if(!$code->WeaponSet1->HasAny() && $code->WeaponSet2->HasAny())
		{
			$code->WeaponSet1 = $code->WeaponSet2;
			$code->WeaponSet2 = new WeaponSet();
		}

		$apiSkills = $aquatic ? $activeBuild->aquatic_skills : $activeBuild->skills;
		$code->SlotSkills->Heal     = $apiSkills->heal         ?? 0;
		$code->SlotSkills->Utility1 = $apiSkills->utilities[0] ?? 0;
		$code->SlotSkills->Utility2 = $apiSkills->utilities[1] ?? 0;
		$code->SlotSkills->Utility3 = $apiSkills->utilities[2] ?? 0;
		$code->SlotSkills->Elite    = $apiSkills->elite        ?? 0;

		switch($code->Profession)
		{
			case Profession::Ranger:
				$rangerData = new RangerData();

				$petBlock = $aquatic ? $activeBuild->pets->aquatic : $activeBuild->pets->terrestrial;
				$rangerData->Pet1 = $petBlock[0] ?? 0;
				$rangerData->Pet2 = $petBlock[1] ?? 0;

				$code->ProfessionSpecific = $rangerData;
				break;

			case Profession::Revenant:
				$revenantData = new RevenantData();

				$legends = $aquatic ? $activeBuild->aquatic_legends : $activeBuild->legends;
				$legend1 = ResolveLegend($code->Specializations->Choice3, $legends[0]);
				$legend2 = ResolveLegend($code->Specializations->Choice3, $legends[1]);
				if($legend1 !== null) // One legend is always set.
				{
					$revenantData->Legend1 = $legend1;
					$revenantData->Legend2 = $legend2 ?? Legend::_UNDEFINED;

					//NOTE(Rennorb): doesnt seem to be available via the api
					// activeBuild.Skills = 
				}
				else // Flip so the legend 1 has the data.
				{
					$revenantData->Legend1 = $legend2;
					$revenantData->Legend2 = Legend::_UNDEFINED;

					$revenantData->AltUtilitySkill1 = $code->SlotSkills->Utility1;
					$revenantData->AltUtilitySkill2 = $code->SlotSkills->Utility2;
					$revenantData->AltUtilitySkill3 = $code->SlotSkills->Utility3;

					// inactive skills dont seem to be available
					$code->SlotSkills->Utility1 = SkillId::_UNDEFINED;
					$code->SlotSkills->Utility2 = SkillId::_UNDEFINED;
					$code->SlotSkills->Utility3 = SkillId::_UNDEFINED;
				}

				$code->ProfessionSpecific = $revenantData;
				break;
		}

		Overrides::PostfixApiBuild($code);

		return $code;
	}

	private static function ResolveStatId($item) : int
	{ return !empty($item->stats) ? $item->stats->id : APICache::ResolveStatId($item->id); }
}
