<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

class PerProfessionData {
	public static $LazyLoadMode = LazyLoadMode::NONE;

	public static PerProfessionData $Guardian    ;
	public static PerProfessionData $Warrior     ;
	public static PerProfessionData $Engineer    ;
	public static PerProfessionData $Ranger      ;
	public static PerProfessionData $Thief       ;
	public static PerProfessionData $Elementalist;
	public static PerProfessionData $Mesmer      ;
	public static PerProfessionData $Necromancer ;
	public static PerProfessionData $Revenant    ;

	public static function __construct_static() {
		PerProfessionData::$Guardian     = new PerProfessionData();
		PerProfessionData::$Warrior      = new PerProfessionData();
		PerProfessionData::$Engineer     = new PerProfessionData();
		PerProfessionData::$Ranger       = new PerProfessionData();
		PerProfessionData::$Thief        = new PerProfessionData();
		PerProfessionData::$Elementalist = new PerProfessionData();
		PerProfessionData::$Mesmer       = new PerProfessionData();
		PerProfessionData::$Necromancer  = new PerProfessionData();
		PerProfessionData::$Revenant     = new PerProfessionData();
	}

	public static function ByProfession(int $profession) : PerProfessionData
	{
		switch ($profession) {
			case Profession::Guardian    : return PerProfessionData::$Guardian    ;
			case Profession::Warrior     : return PerProfessionData::$Warrior     ;
			case Profession::Engineer    : return PerProfessionData::$Engineer    ;
			case Profession::Ranger      : return PerProfessionData::$Ranger      ;
			case Profession::Thief       : return PerProfessionData::$Thief       ;
			case Profession::Elementalist: return PerProfessionData::$Elementalist;
			case Profession::Mesmer      : return PerProfessionData::$Mesmer      ;
			case Profession::Necromancer : return PerProfessionData::$Necromancer ;
			case Profession::Revenant    : return PerProfessionData::$Revenant    ;
			default: throw new \InvalidArgumentException("profession");
		};
	}

	private \DateTime $_lastUpdate;

	private function __construct() {
		$this->_lastUpdate = new \DateTime("1970-01-01");
	}

	/** @remarks Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. */
	public $PalletteToSkill = [];
	/** @remarks Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. */
	public $SkillToPallette = [];
	/** @remarks Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. Indices are offset by 1. */
	public $IndexToId = [];
	/** @remarks Once loaded also converts 0 &lt;-&gt; 0 for _UNDEFINED passthrough. Indices are offset by 1. */
	public $IdToIndex = [];

	public function TryInsertSkill(int $palletteId, int $skillId) : bool
	{
		$good1 = !array_key_exists($palletteId, $this->PalletteToSkill);
		$good2 = !array_key_exists($skillId, $this->SkillToPallette);
		if($good1 && $good2)
		{
			$this->PalletteToSkill[$palletteId] = $skillId;
			$this->SkillToPallette[$skillId] = $palletteId;
			return true;
		}
		return false;
	}

	public function TryInsertSpec(int $professionSpecIndex, int $specId) : bool
	{
		$good1 = !array_key_exists($professionSpecIndex, $this->IndexToId);
		$good2 = !array_key_exists($specId, $this->IdToIndex);
		if($good1 && $good2)
		{
			$this->IndexToId[$professionSpecIndex] = $specId;
			$this->IdToIndex[$specId] = $professionSpecIndex;
			return true;
		}
		return false;
	}

	public function AssignSkill(int $palletteId, int $skillId) : void
	{
		$this->PalletteToSkill[$palletteId] = $skillId;
		$this->SkillToPallette[$skillId] = $palletteId;
	}

	public function AssignSpec(int $professionSpecIndex, int $specId) : void
	{
		$this->IndexToId[$professionSpecIndex] = $specId;
		$this->IdToIndex[$specId] = $professionSpecIndex;
	}

	//NOTE(Rennorb): cant trim arrays in php

	function ReloadFromOfflineFiles(int $profession) : void
	{
		$professionName = Profession::TryGetName($profession);
		$file = fopen("offline/pallettes/$professionName.csv", 'r', use_include_path: true);
		while(($line = fgets($file)) !== false)
		{
			list($palletteId_s, $skillId_s) = explode(';', $line, 2);
			$this->TryInsertSkill(intval($palletteId_s), intval($skillId_s));
		}

		$file = fopen("offline/specializations/$professionName.csv", 'r', use_include_path: true);
		while(($line = fgets($file)) !== false)
		{
			list($index_s, $specialization_s) = explode(';', $line, 2);
			$this->TryInsertSpec(intval($index_s) + 1, intval($specialization_s));
		}
	}

	/** @remarks This will only ever add new entries, never remove them. */
	public static function ReloadAll(bool $skipOnline = false) : void
	{
		//TODO(Rennorb): make parallel
		for($profession = Profession::_FIRST(); $profession <= 9; $profession++) {
			if($profession === Profession::_UNDEFINED) continue;

			PerProfessionData::Reload($profession, $skipOnline);
		}
	}

	/** @remarks This will only ever add new entries, never remove them. */
	public static function Reload(int $profession, bool $skipOnline = false) : void
	{
		$targetData = PerProfessionData::ByProfession($profession);

		if((new \DateTimeImmutable())->getTimestamp() - $targetData->_lastUpdate->getTimestamp() < 5 * 60) return;

		if(count($targetData->PalletteToSkill) === 0)
		{
			//NOTE(Rennorb): php doesn't allow preallocation

			$targetData->TryInsertSkill(0, SkillId::_UNDEFINED);
			$targetData->TryInsertSpec(0, SpecializationId::_UNDEFINED);
		}

		$loaded = false;
		if(!$skipOnline)
		{
			$professionName = Profession::TryGetName($profession);
			try
			{
				$professionData = APICache::Get("/professions/$professionName", '2019-12-19T00:00:00.000Z');
				foreach($professionData->skills_by_palette as [$palette, $skill])
				{
					$targetData->AssignSkill($palette, $skill);
				}
				$i = 1;
				foreach($professionData->specializations as $specId)
				{
					$targetData->AssignSpec($i++, $specId);
				}
				$loaded = true;
			}
			catch(\Error $ex)
			{
				trigger_error("Could not fetch skill pallette for $professionName, will fall back to offline list.\n{$ex}", E_USER_WARNING);
			}
		}

		if(!$loaded) {
			$targetData->ReloadFromOfflineFiles($profession);
		}

		Overrides::LoadAdditionalPerProfessionData($profession, $targetData);

		//NOTE(Rennorb): no trimming either
		//$targetData->TrimExcess();

		$targetData->_lastUpdate = new \DateTime();
	}
}
