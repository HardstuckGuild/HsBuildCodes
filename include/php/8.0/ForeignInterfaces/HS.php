<?php namespace Hardstuck\GuildWars2\BuildCodes\V2\Foreign\HS;

use Hardstuck\GuildWars2\BuildCodes\V2\APICache;
use Hardstuck\GuildWars2\BuildCodes\V2\BuildCode;
use Hardstuck\GuildWars2\BuildCodes\V2\Kind;
use Hardstuck\GuildWars2\BuildCodes\V2\LazyLoadMode;
use Hardstuck\GuildWars2\BuildCodes\V2\PerProfessionData;
use Hardstuck\GuildWars2\BuildCodes\V2\Profession;
use Hardstuck\GuildWars2\BuildCodes\V2\RangerData;
use Hardstuck\GuildWars2\BuildCodes\V2\RevenantData;
use Hardstuck\GuildWars2\BuildCodes\V2\SpecializationId;
use Hardstuck\GuildWars2\BuildCodes\V2\Statics;
use Hardstuck\GuildWars2\BuildCodes\V2\TextLoader;

function ConvertToHsArray(BuildCode $code, string $codeText)
{
	if (!empty($build['equipment']['weaponset1'][0])) {
		if (empty($build['weapon_skills'][0][0])) {
				$equipw2 = $build['equipment']['weaponset1'][0]; 
		} else {
				$equipw1 = $build['equipment']['weaponset1'][0]; 
		}
	}
	if(!$equipw2 && !empty($build['equipment']['weaponset1'][1])) {$equipw2 = $build['equipment']['weaponset1'][1]; }

	if (!empty($build['equipment']['weaponset2'][0])) {
			if (empty($build['weapon_skills'][1][0])) {
					$equipw4 = $build['equipment']['weaponset2'][0]; 
			} else {
					$equipw3 = $build['equipment']['weaponset2'][0]; 
			}				
	}
	if(!$equipw4 && !empty($build['equipment']['weaponset2'][1])) {$equipw4 = $build['equipment']['weaponset2'][1]; }

	if (!empty($build['weapon_types'][0][0])) {
			if (empty($build['weapon_skills'][0][0])) {
					$wtype2 = $build['weapon_types'][0][0];
			} else {
					$wtype1 = $build['weapon_types'][0][0];
					
			}
	}
	if(!$wtype2 && !empty($build['weapon_types'][0][1])) $wtype2 = $build['weapon_types'][0][1];

	if (!empty($build['weapon_types'][1][0]))  {
			if (empty($build['weapon_skills'][1][0])) {
					$wtype4 = $build['weapon_types'][1][0];
			} else {
					$wtype3 = $build['weapon_types'][1][0];
			}				
	}
	if(!$wtype4 && !empty($build['weapon_types'][1][1])) $wtype4 = $build['weapon_types'][1][1];

	if (!empty($build['equipment']['sigils'][0])) {
			if(!$wtype1) {
					$equips2 = $build['equipment']['sigils'][0]; 
			} else {
					$equips1 = $build['equipment']['sigils'][0]; 
			}
	}
	if(!$equips2 && !empty($build['equipment']['sigils'][1])) $equips2 = $build['equipment']['sigils'][1];
	if (!empty($build['equipment']['sigils'][2])) {
			if(!$wtype3) {
					$equips4 = $build['equipment']['sigils'][2]; 
			} else {
					$equips3 = $build['equipment']['sigils'][2]; 
			}
	}
	if(!$equips4) $equips4 = (empty($build['equipment']['sigils'][3])) ? NULL : $build['equipment']['sigils'][3];

	$duplicateWeaponSets = (isset($build['duplicateWeaponSets']) && $build['duplicateWeaponSets']==true) ? 1 : 0;
	$oneMainOneOff = (isset($build['oneMainOneOff']) && $build['oneMainOneOff']==true) ? 1 : 0;

	for ($i = 1; $i <= 4; $i++){
			${'wt' . $i} = (isset(${'wtype' . $i}) && weapon_replace[${'wtype' . $i}]) ? weapon_replace[${'wtype' . $i}] : NULL;
			
	}
	for ($i = 1; $i <= 6; $i++){
			${'rune' . $i} = (isset(runes_replace[$build['equipment']['runes'][$i-1]])) ? runes_replace[$build['equipment']['runes'][$i-1]] : $build['equipment']['runes'][$i-1];
	}

	for ($i = 1; $i <= 4; $i++){
			${'sigil' . $i} = (isset(sigils_replace[${'equips' . $i}])) ? sigils_replace[${'equips' . $i}] : ${'equips' . $i};
			
	}


	if ($build['specializations'][2]['id'] == 59 && empty($build['skills']['elites'][0])) {
					$build['skills']['elites'][0] = '45449';
	}

	PerProfessionData::$LazyLoadMode = LazyLoadMode::FULL;
	$ingameLink = TextLoader::WriteOfficialBuildCode($build);

	$sql_data = array(
			'hs_code'            => $codeText,
			'gw2code'            => $ingameLink,
			'gamemode'           => Kind::TryGetName($code->Kind),
			'gamemode_id'        => $code->Kind,
			'profession'         => Profession::TryGetName($code->Profession),
			'profession_id'      => $code->Profession,
			'spec1_id'           => $code->Specializations->Choice1->SpecializationId,
			'spec1_name'         => SpecializationId::TryGetName($code->Specializations->Choice1->SpecializationId),
			'spec1_trait1'       => APICache::ResolveTrait($code->Specializations->Choice1, TraitSlot::Adept),
			'spec1_trait2'       => APICache::ResolveTrait($code->Specializations->Choice1, TraitSlot::Master),
			'spec1_trait3'       => APICache::ResolveTrait($code->Specializations->Choice1, TraitSlot::GrandMaster),
			'spec2_id'           => $code->Specializations->Choice2->SpecializationId,
			'spec2_name'         => SpecializationId::TryGetName($code->Specializations->Choice2->SpecializationId),
			'spec2_trait1'       => $build['specializations'][1]['traits'][0],
			'spec2_trait2'       => $build['specializations'][1]['traits'][1],
			'spec2_trait3'       => $build['specializations'][1]['traits'][2],
			'spec3_id'           => $code->Specializations->Choice3->SpecializationId,
			'spec3_name'         => SpecializationId::TryGetName($code->Specializations->Choice3->SpecializationId),
			'spec3_trait1'       => $build['specializations'][2]['traits'][0],
			'spec3_trait2'       => $build['specializations'][2]['traits'][1],
			'spec3_trait3'       => $build['specializations'][2]['traits'][2],
			'heal1'              => $build['skills']['heals'][0],
			'heal2'              => $build['skills']['heals'][1],
			'utility1'           => $build['skills']['utilities'][0],
			'utility2'           => $build['skills']['utilities'][1],
			'utility3'           => $build['skills']['utilities'][2],
			'utility4'           => $build['skills']['utilities'][3],
			'utility5'           => $build['skills']['utilities'][4],
			'utility6'           => $build['skills']['utilities'][5],
			'elite1'             => $build['skills']['elites'][0],
			'elite2'             => $build['skills']['elites'][1],
			'weapon1_s1'         => $build['weapon_skills'][0][0],
			'weapon1_s2'         => $build['weapon_skills'][0][1],
			'weapon1_s3'         => $build['weapon_skills'][0][2],
			'weapon1_s4'         => $build['weapon_skills'][0][3],
			'weapon1_s5'         => $build['weapon_skills'][0][4],
			'weapon2_s1'         => $build['weapon_skills'][1][0],
			'weapon2_s2'         => $build['weapon_skills'][1][1],
			'weapon2_s3'         => $build['weapon_skills'][1][2],
			'weapon2_s4'         => $build['weapon_skills'][1][3],
			'weapon2_s5'         => $build['weapon_skills'][1][4],
			'equipment_weapon1'  => $equipw1,
			'equipment_weapon2'  => $equipw2,
			'equipment_weapon3'  => $equipw3,
			'equipment_weapon4'  => $equipw4,
			'equipment_armor1'   => $code->EquipmentAttributes->Helmet,
			'equipment_armor2'   => $code->EquipmentAttributes->Shoulders,
			'equipment_armor3'   => $code->EquipmentAttributes->Chest,
			'equipment_armor4'   => $code->EquipmentAttributes->Gloves,
			'equipment_armor5'   => $code->EquipmentAttributes->Leggings,
			'equipment_armor6'   => $code->EquipmentAttributes->Boots,
			'equipment_trinket1' => $code->EquipmentAttributes->BackItem,
			'equipment_trinket2' => $code->EquipmentAttributes->Accessory1,
			'equipment_trinket3' => $code->EquipmentAttributes->Accessory2,
			'equipment_trinket4' => $code->EquipmentAttributes->Amulet,
			'equipment_trinket5' => $code->EquipmentAttributes->Ring1,
			'equipment_trinket6' => $code->EquipmentAttributes->Ring2,
			'equipment_rune1'    => $code->Rune,
			'equipment_rune2'    => $code->Rune,
			'equipment_rune3'    => $code->Rune,
			'equipment_rune4'    => $code->Rune,
			'equipment_rune5'    => $code->Rune,
			'equipment_rune6'    => $code->Rune,
			'equipment_sigil1'   => $code->WeaponSet1->Sigil1,
			'equipment_sigil2'   => $code->WeaponSet1->Sigil2,
			'equipment_sigil3'   => $code->WeaponSet2->Sigil1,
			'equipment_sigil4'   => $code->WeaponSet2->Sigil2,
			'pvp_amulet'         => $code->EquipmentAttributes->Amulet,
			'pvp_rune'           => $code->Rune,
			'weapon_type1'       => $wtype1,
			'weapon_type2'       => $wtype2,
			'weapon_type3'       => $wtype3,
			'weapon_type4'       => $wtype4,
			'weapon_id1'         => $wt1,
			'weapon_id2'         => $wt2,
			'weapon_id3'         => $wt3,
			'weapon_id4'         => $wt4,
			'pet1'               => $code->Profession === Profession::Ranger ? $code->ProfessionSpecific->Pet1 : null,
			'pet2'               => $code->Profession === Profession::Ranger ? $code->ProfessionSpecific->Pet1 : null,
			'legend1'            => $code->Profession === Profession::Revenant ? $code->ProfessionSpecific->Legend1 : null,
			'legend2'            => $code->Profession === Profession::Revenant ? $code->ProfessionSpecific->Legend2 : null,
			'one_main_one_off'   => $oneMainOneOff,
			'duplicate_weapon_sets' => $duplicateWeaponSets,
	);

	global $wpdb;
	$insertion = (HS_GW2_FORCE_BUILDS_REWRITE) ? $wpdb->replace(HS_GW2_BUILDS_DB, $sql_data) : $wpdb->insert(HS_GW2_BUILDS_DB, $sql_data); 
	return ($insertion) ? $sql_data : false;
}

const weapon_replace = array(
	'Axe' => '91622',
	'Dagger' => '91636',
	'Mace' => '91671',
	'Pistol' => '91675',
	'Scepter' => '91619',
	'Sword' => '91649',
	'Focus' => '91657',
	'Shield' => '91633',
	'Torch' => '91653',
	'Warhorn' => '91676',
	'Greatsword' => '91646',
	'Hammer' => '91652',
	'Longbow' => '91632',
	'Rifle' => '91637',
	'Shortbow' => '91628',
	'Staff' => '91674',
	'Harpoon Gun' => '90283',
	'Spear' => '90226',
	'Trident' => '90637'
);

const runes_replace = array(
	'91638' => '38206',
	'91641' => '48907',
	'91513' => '24765',
	'91428' => '24732',
	'91444' => '73653',
	'91529' => '24768',
	'91392' => '67344',
	'91417' => '44951',
	'91482' => '89999',
	'91468' => '24779',
	'91564' => '24729',
	'91608' => '24703',
	'91530' => '70600',
	'91471' => '24776',
	'91566' => '24771',
	'91556' => '24708',
	'91639' => '81091',
	'91399' => '24860',
	'91550' => '44957',
	'91587' => '67342',
	'91503' => '24717',
	'91572' => '24726',
	'91411' => '49460',
	'91605' => '24857',
	'91553' => '24738',
	'91512' => '68437',
	'91401' => '24720',
	'91423' => '24714',
	'91591' => '76813',
	'91494' => '24794',
	'91576' => '24830',
	'91460' => '24687',
	'91547' => '24750',
	'91602' => '24845',
	'91477' => '24854',
	'91397' => '71425',
	'91425' => '24833',
	'91396' => '83367',
	'91599' => '24788',
	'91565' => '73399',
	'91588' => '24741',
	'91457' => '72852',
	'91483' => '82791',
	'91404' => '67912',
	'91459' => '24699',
	'91432' => '74978',
	'91430' => '70450',
	'91433' => '24723',
	'91493' => '24744',
	'91465' => '24800',
	'91522' => '24812',
	'91489' => '24747',
	'91538' => '83338',
	'91507' => '24797',
	'91408' => '24696',
	'91419' => '24851',
	'91391' => '24785',
	'91475' => '24735',
	'91570' => '24824',
	'91585' => '76100',
	'91560' => '82633',
	'91580' => '24753',
	'91435' => '24762',
	'91533' => '24688',
	'91581' => '36044',
	'91387' => '24803',
	'91568' => '84127',
	'91501' => '24842',
	'91567' => '24806',
	'91410' => '24848',
	'91497' => '24756',
	'91592' => '24702',
	'91557' => '24782',
	'91541' => '24815',
	'91578' => '70829',
	'91627' => '84171',
	'91573' => '83502',
	'91447' => '69370',
	'91595' => '24836',
	'91464' => '83663',
	'91582' => '71276',
	'91510' => '83964',
	'91590' => '84749',
	'91673' => '85713',
	'91515' => '47908',
	'91583' => '76166',
	'91579' => '24818',
	'91508' => '67339',
	'91485' => '24691',
	'91551' => '24827',
	'91445' => '24757',
	'91523' => '24821',
	'91518' => '24839',
	'91451' => '83423',
	'91593' => '24791',
	'91525' => '88118',
	'91381' => '72912',
	'91516' => '44956',
	'91545' => '24711',
);

const sigils_replace = array(
	'91589' => '72872',
	'91607' => '24618',
	'91403' => '72092',
	'91534' => '24612',
	'91520' => '24554',
	'91390' => '24601',
	'91382' => '24584',
	'91542' => '67913',
	'91604' => '24570',
	'91476' => '24575',
	'91492' => '81045',
	'91416' => '44944',
	'91546' => '24865',
	'91519' => '24645',
	'91584' => '24630',
	'91548' => '67340',
	'91473' => '72339',
	'91496' => '24578',
	'91603' => '67341',
	'91575' => '24636',
	'91431' => '24664',
	'91388' => '24583',
	'91539' => '24654',
	'91480' => '24609',
	'91544' => '70825',
	'91452' => '24681',
	'91531' => '24560',
	'91537' => '24661',
	'91441' => '24607',
	'91559' => '24548',
	'91439' => '24615',
	'91443' => '24567',
	'91461' => '82876',
	'91441' => '24607',
	'91559' => '24548',
	'91439' => '24615',
	'91443' => '24567',
	'91461' => '82876',
	'91558' => '38294',
	'91552' => '24605',
	'91455' => '24809',
	'91436' => '24648',
	'91511' => '24627',
	'91438' => '91339',
	'91406' => '24597',
	'91535' => '24555',
	'91490' => '24651',
	'91405' => '24868',
	'91463' => '67343',
	'91600' => '24678',
	'91413' => '37912',
	'91577' => '24599',
	'91502' => '24582',
	'91393' => '24591',
	'91453' => '24672',
	'91478' => '44950',
	'91543' => '68436',
	'91521' => '49457',
	'91474' => '24572',
	'91506' => '24655',
	'91398' => '24639',
	'91426' => '24580',
	'91500' => '24621',
	'91509' => '24571',
	'91420' => '24561',
	'91486' => '73532',
	'91400' => '44947',
	'91415' => '24594',
	'91429' => '71130',
	'91456' => '24658',
	'91499' => '84505',
	'91488' => '24624',
	'91609' => '24675',
	'91526' => '24684',
	'91409' => '24589',
	'91470' => '24592',
	'91561' => '24562',
	'91389' => '36053',
	'91384' => '86170',
	'91412' => '48911',
	'91448' => '74326',
	'91524' => '24642',
	'91532' => '24632',
	'91407' => '24600',
	'91594' => '24551',
	'91527' => '24667',
);