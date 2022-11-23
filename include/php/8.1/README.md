# Hardstuck BuildCodes V2 - PHP 8.1 implementation

There is an autoloader available: 
```php
require_once 'include/php/8.1/autoload.php';
```

### There are multiple ways to obtain a Hardstuck BuildCode Object:

Some of these functions require to call `PerProfessionData::Reload[All]()` first or set `PerProfessionData::LazyLoadMode` to a value other than `NONE`.

```php
use Hardstuck\GuildWars2\BuildCodes\V2\TextLoader;

// From existing build textual codes:
$code1 = TextLoader::LoadBuildCode("CoI___~______A~M~__A_");

// From existing textual in-game chatlinks:
$inGameLink = "[&DQkAAAAARQDcEdwRAAAAACsSAADUEQAAAAAAAAQCAwDUESsSAAAAAAAAAAA=]";
$code3 = TextLoader::LoadOfficialBuildCode($inGameLink);
```

```php
use Hardstuck\GuildWars2\BuildCodes\V2\BinaryLoader;

// From existing binary in-game chatlink data:
$inGameLink = "[&DQkAAAAARQDcEdwRAAAAACsSAADUEQAAAAAAAAQCAwDUESsSAAAAAAAAAAA=]";
$rawInGameBytes = base64_decode(substring($inGameLink, 2, strlen($inGameLink) - 3));
$code4 = BinaryLoader::LoadOfficialBuildCode($rawInGameBytes);
```

```php
use Hardstuck\GuildWars2\BuildCodes\V2\APILoader;

// From an existing player via the official api:
$apiKey = "AD041D99-AEEF...";
$characterName = "Don Joe Was Taken";
$code5 = APILoader::LoadBuildCode($apiKey, $characterName, Kind::PvE);
```

### From this Object different kinds of codes can be generated:

```php
use Hardstuck\GuildWars2\BuildCodes\V2\TextLoader;
use Hardstuck\GuildWars2\BuildCodes\V2\BinaryLoader;

$code = SomeSource();

// To a textual version of the Hardstuck BuildCode
$hsCode = TextLoader::WriteBuildCode($code);

// To a compressed binary version of the Hardstuck BuildCode
$buffer = BinaryLoader::WriteBuildCode($code);

// Directly to a full in-game chat link
$chatLink = TextLoader::WriteOfficialBuildCode($code);

// To the raw data of an in-game chat link
$buffer = BinaryLoader::WriteOfficialBuildCode($code);
```

There are multiple overloads available for most of those operations.