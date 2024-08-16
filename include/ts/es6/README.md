# Hardstuck BuildCodes V2 - TS ES6 implementation

### There are multiple ways to obtain a Hardstuck BuildCode Object:

Some of these functions require to call `PerProfessionData.Reload[All]()` first or set `PerProfessionData.LazyLoadMode` to a value other than `NONE`.

```ts
import TextLoader from 'include/ts/TextLoader';

// From existing build textual codes:
const code1 = TextLoader.LoadBuildCode("CoI___~______A~M~__A_");

// From existing textual in-game chatlinks:
const inGameLink = "[&DQkAAAAARQDcEdwRAAAAACsSAADUEQAAAAAAAAQCAwDUESsSAAAAAAAAAAA=]";
const code3 = await TextLoader.LoadOfficialBuildCode(inGameLink);
```

```ts
import BinaryLoader from 'include/ts/BinaryLoader';
import { Base64Decode } from 'include/ts/Util/Static';

// From existing binary in-game chatlink data:
const inGameLink = "[&DQkAAAAARQDcEdwRAAAAACsSAADUEQAAAAAAAAQCAwDUESsSAAAAAAAAAAA=]";
const rawInGameBytes = Base64Decode(inGameLink.slice(2, -1));
const code4 = await BinaryLoader.LoadOfficialBuildCode(rawInGameBytes);
```

```ts
import APILoader from 'include/ts/APILoader';

// From an existing player via the official api:
const apiKey = "AD041D99-AEEF...";
const characterName = "Don Joe Was Taken";
const code5 = await APILoader.LoadBuildCode(apiKey, characterName, Kind.PvE);
```

### From this Object different kinds of codes can be generated:

```ts
import TextLoader from 'include/ts/TextLoader';
import BinaryLoader from 'include/ts/BinaryLoader';

const code = SomeSource();

// To a textual version of the Hardstuck BuildCode
const hsCode = TextLoader.WriteBuildCode(code);

// To a compressed binary version of the Hardstuck BuildCode
const buffer = BinaryLoader.WriteBuildCode(code);

// Directly to a full in-game chat link
const chatLink = await TextLoader.WriteOfficialBuildCode(code);

// To the raw data of an in-game chat link
const buffer = await BinaryLoader.WriteOfficialBuildCode(code);
```

There are multiple overloads available for most of those operations.

### Other utility functions available:

```ts
Static.IsTwoHanded(weapon : WeaponType) : boolean;
Static.IsAquatic(weapon : WeaponType) : Tristate;
Static.ResolveEffectiveWeapons(code : BuildCode, set : WeaponSetNumber) : WeaponSet;
Static.ResolveDummyItemForWeaponType(weaponType : WeaponType, statId : StatId) : ItemId;
Static.ResolveDummyItemForEquipment(equipmentIndex : int, weightClass : WeightClass, statId : StatId) : ItemId;
Static.ResolveWeightClass(profession : Profession) : WeightClass;

APICache.ResolveWeaponType(itemId : int) : Promise<WeaponType>;
APICache.ResolveStatId(itemId : int) : Promise<StatId>;
APICache.ResolvePosition(traitId : int) : Promise<TraitLineChoice>;
APICache.ResolveWeaponSkill(code : BuildCode, effectiveWeapons : WeaponSet, skillIndex : int) : Promise<SkillId>;
APICache.ResolveTrait(spec : Specialization, traitSlot : TraitSlot) : Promise<TraitId>;
```