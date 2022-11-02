# HS BuildCode v2 Spec RFC

## Ethos
The following considerations were taken into account in descending priority:

1. Printable characters only: The codes are meant to be used in chats and urls, so the codes must only use standard printable ASCII/UTF8 characters. They should avoid 'special characters' as to not break urls (_or commonly used url highlighters/parsers!_) that contain them. This basically means the character pool is reduced to the alphanumerical set (character code 0x30 - 0x39, 0x41 - 0x5A, 0x6A - 0x7A).
4. Completeness: The codes must cover all possible scenarios. This includes all gamemodes, professions, (elite-)specializations, trait choices, weapon sets (weapon types), slot skills, stat attributes, sigils, runes, infusions and all special class features. [RFC] Rune mixing was deemed irrelevant. [/RFC]
3. Compactness: The codes must be as short as possible. Allow omission of fields where sensible.
2. Robustness: The codes should be as robust to further game (content-) updates as possible. It is, however, quite hard to anticipate game-mechanical updates and the changes introduced by those. This mostly boils down to use or enforce stable ordering wherever possible and using sufficiently large fields to store ids that won't overflow in the forseeable future.

## Textual spec

### Some notes for some of the (de-)compression routines:
Let character_set be `ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-`
This is also the character set used for base64 encoding. It is identical to the default set except the last character, which is usually a `/` (slash). This character however has special meaning in urls and is therefore problematic, so it is replaced by a `-` (minus). Something similar could be said about the plus sign, however, this one works fine in query parameters. The only issue with this is that server frameworks might automatically decode request uris which will result in plusses being converted to spaces. This however doesn't really pose a problem and is just something to be aware of. We require exactly 64 characters in the set for encoding trait choices, and two more characters (currently using `_` (underscore) and `~` (tilde)) to indicate certain empty conditions. This completely exhausts the url character set, except `.` (period). The issue with this character is that url highlighters tend to only treat periods as part of urls if they aren't at the end of the url, so https://my-site.com/path/?query=adasd. does not contain the last period, as it assumes its supposed to end a sentence. This is a problem in our case since the code might end in either an id or emptiness indicator, so we cant use the period in either of those without breaking links. 

Small indices are directly encoded with the aforementioned `character_set[index]`. Indices greater than that are encoded by encode defined below, which will also be mentioned in places where its used.
```
encode(index, len):
  result := "";
	while(index > 0)
		result = result + character_set[index & 0b00111111];
		index = index >>> 6;
	if(length(result) < len)
		result = result + '~';
	return result;
```
With `+` being concatenation and `>>>` being the sign preserving shift right.
This is basically base64 with a set result length and early exit code. The encoding for indices below 64 could also be expressed as `encode(index, 1)`.

All references to an API without hostname (e.g: `/v2/pets`) reference the official gw2 api.

The first line contains a v1 buildcode with only two weapons and uniform stats for width comparison. It is aligned to cover the width of fields that would be present, not rearranged to match the fields that exist in v1.
```
a c e ccabbc bgcbbcSUT       R              ge-cfc-cfm_GTxg _if t_ik     m        +  +
V T P STSTST WS..wS..WS..wS..WS..S..wS..S.. S..S..S..S..S.. R.. A..n,,,, I..n,,,, F..U.. A,,,,,
```

`[V]` Version [1 character. currently `B`] used for backwards compatibility.
  - Not contained in codes below version `B`.
  - Lower case letters indicate binary format base64 encoded.

`[T]` Type [1 character] `p`: pvp, `w`: wvw, `o`: other(pve)

`[P]` Profession [1 character] resolved by `/v2/professions`
  - `A-I`: Guardian, Warrior, Engineer, Ranger, Thief, Elementalist, Mesmer, Necromancer, Revenant

`[STSTST]` Specializations [3 * 2 characters] pairs of (1 char specialization + 1 char selected traits):
  - `_` (underscore): empty trait line
  - `A-H`: specialization index depending on the profession. Spec index is resolved by `/v2/professions/<profession>$specializations` after ordering by id ascending
  - Trait choices [6 characters] encoded as follows:
    1. Let `pos(c) : {1, 2, 3} => {0, 1, 2, 3}` be the position of the selected trait in the current trait line in column `c` as it appears in game: `pos(c) := { 0 if empty, 1 if top, 2 if mid, 3 if bottom }`.
    2. Calculate an index by `index = 0; for(c: 1..3) index |= pos(c) << (6 - c * 2)`.

        This effectively constructs `0b00aabbcc` with `aa` = pos of first choice, `bb` = pos of second choice, `cc` = pos of third choice. With a max value of 63 this can be used to index `character_set` and obtain the final encoding. Omit if trait line is empty.

`[WS..wS..WS..wS..]` Weapons [2 * 3-8 characters] pairs of (1 char weapon type id, 1-3 char sigil id, 0-1 char weapon type id, 1-3 char sigil id):
  - if the first weapon is two handed, the second weapon id in the set is omitted 
  1. - `_` (underscore): empty weapon slot
     - `A-T`: weapon type id (resolved by `hardstuck.gg/api/weapon_types`)
  2. - `_` (underscore): empty sigil slot
     - `1-3 characters`: sigil id resolved by `/v2/items`, `encode(id, 3)`

  - The second weapon set may be omitted from the code by replacing 
the whole second set (`[WS..wS..]`) with a `~` (tilde). This section can be
omitted completely by replacing the whole section with a single `~` (tilde).

`[S..S..S..S..S..]` Slot skills [5 * 1-3 characters] each:
  - `_` (underscore): empty skill slot
  - `1-3 characters`: skill id resolved by `/v2/skills`, `encode(id, 3)`

`[R..]` Rune [1-3 characters]. Always the same for all slots.
 - `_` (underscore): if empty
 - `1-3 characters`: item id resolved by `/v2/items`, `encode(id, 3)`

`[A..n,,,,]` Attribute ids [1-n characters] itemstat id resolved by `/v2/itemstats`
  - For pvp codes encode the stat id of the amulet with `encode(id, 2)`
  - For other gamemodes:
    1. Order the equipment in the following way: armor (helmet to boots), backpiece, accessory1, accessory2, ring1, ring2, weapons (main hand then offhand for all sets), amulet.
       - Only include weapons in this list that actually exist in the code.
    2. Going trough that list `encode(itemstat_id, 2)` of the current item,
    3. then append `A-O` (one char) for the amount of times this stat type repeats in the list. 
       - Omit the repetition count if there is only one item left as it would always be 1. 
    4. Repeat step 2. and 3. until the whole list has been processed.

`[I..n,,,,]` Infusions [1-n characters]
  - For pvp codes encodes omit this section completely.
  - To omit infusions replace this section with a single `~` (tilde).
  - Encoded in the same way as attribute ids, except with `encode(item_id, 3)`.
  - Use `_` (underscore) to encode an empty slot, this also requires the usual repetition indicator.

`[F..U..]` Food + Utility [2-6 characters] pair of 1-3 characters, each:
	- For pvp codes encodes omit this section completely.
  - `_` (underscore): empty food or utility
  - `1-3 characters`: item id resolved by `/v2/items`, `encode(id, 3)`

`[A,,,,,]` Arbitrary data [0-n characters], currently used for:
  - Ranger pets: [1-2 characters]
    - assume the following pet order: pet1, pet2
    - `~` (tilde): omit pets
    - `_` (underscore): empty pet slot
    - `2 characters`: `encode(pet_id, 2)` from `/v2/pets`
  - Revenant Legends + utility: [3-11 characters]
    1. 2 times 
       - `_` (underscore): empty legend slot
       - `A-F` : legend index from `/v2/legends` to index `character_set`. Shiro, Glint, Mallyx, Jalis, Ventari, Kalla, Vindicator
    2. 3 times 
       - `_` (underscore): empty alternate legend utility skill slot
       - `1-3 characters`: alternate legend utility skill id resolved by `/v2/skills`, `encode(id, 3)`
       - omit whole block if second legend is empty
    
### A note on underwater Codes:
There are no special fields defined for handling underwater data. To define underwater data just make the following adjustments:
  - Weapon 1 of set1 becomes the first UW weapon, weapon 1 of set2 becomes the second UW weapon.
	- The helmet is replaced by the aquabreather.
  - Ranger Pets now define the underwater pets.
	- Revenant legends and alternate skills now define the UW legends / skills.

There is no signal build into a code to determine if a code is an underwater code, as there aren't really any functional differences. The end user may however inspect the weapons, if present, to reach a conclusion.

## Binary (compressed) spec

This can be base64 encoded. Doing so will expand the code by about 1/3 of its length.

Field widths are measured in bits. See textual specification for details on how to obtain numeric values and when to omit fields. Fields use big endianess.

```
8 : Version char
2 : Type: p: 0, w: 1, o: 2
4 : Profession index 

repeat 3
	4 : specializations: 0 if trait line is empty, 1 + index otherwise
	either
		omitted
	or
		repeat 3
			2 : selected trait position. 0 if nothing selected, position otherwise

either
	5 : 0 if code does not contain weapons 
or
	5  : set1 main hand weapon. 1 if slot is empty, 2 + weapon type id otherwise
	24 : slot 1 sigil. 0 if no sigil in slot1, 1 + sigil item id otherwise
	5  : set1 offhand weapon. 1 if slot is empty, 2 + weapon type id otherwise. omit this if set1 main hand is two handed
	24 : slot 2 sigil. 0 if no sigil in slot2, 1 + sigil item id otherwise
	either
		5 : 0 if code des not contain second weapon set
	or
		5  : set2 main hand weapon. 1 if slot is empty, 2 + weapon type id otherwise
		24 : slot 1 sigil. 0 if no sigil in slot1, 1 + sigil item id otherwise
		5  : set2 offhand weapon. 1 if slot is empty, 2 + weapon type id otherwise. omitted if set2 main hand is two handed
		24 : slot 2 sigil. 0 if no sigil in slot2, 1 + sigil item id otherwise


repeat 5
	24 : 1 + Skill ids, 0 if empty

24 : 1 + Rune id, 0 if empty

either
	16 : stat id (when type = pvp)
or
	dynamic repeat
		16 : stat id
		4  : repeat count

either
	24 : 0 if infusions omitted
or
	dynamic repeat
		24 : 1 if empty slot, 2 + infusion item id otherwise
		4  : repeat count

24 : food item. 0 if none, 1 + item_id otherwise, omit for pvp codes
24 : utility item. 0 if none, 1 + item_id otherwise, omit for pvp codes

either
	no additional data
or
	7 : pet 1. 0 to omit block, 1 for empty slot, 2 + pet_id otherwise
	7 : pet 2. 1 for empty slot, 2 + pet_id otherwise, omit if pet 1 is 0
or
	4 : legend 1. 1 + legend_id otherwise
	4 : legend 2. 0 for empty slot, 1 + legend_id otherwise

	repeat 3. alternate legend skills. omit if legend 2 is empty
		24 : 1 + Skill ids, 0 if empty
```
(406 <-> 482) bits / 8 * 4/3 = (68 <-> 81) chars.
Interestingly only about 12 chars (~ 20%) less than the textual representation, the encoding _does_ expand it a lot.