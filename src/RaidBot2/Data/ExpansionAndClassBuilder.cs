namespace RaidBot2.Data;

internal class ExpansionAndClassBuilder
{
    private readonly PlayerClass[] _classes = [
        new() { Id = 01, Name = "Druid", Icon = "<:druid:945492995321528370>", SearchTerms = "druid" },
        new() { Id = 02, Name = "Hunter", Icon = "<:hunter:945492995417972736>", SearchTerms = "hunter;huntard" },
        new() { Id = 03, Name = "Mage", Icon = "<:mage:945492995225030697>", SearchTerms = "mage" },
        new() { Id = 04, Name = "Paladin", Icon = "<:paladin:945492995321495582>", SearchTerms = "paladin;pala;pally" },
        new() { Id = 05, Name = "Priest", Icon = "<:priest:945492995602526238>", SearchTerms = "priest" },
        new() { Id = 06, Name = "Rogue", Icon = "<:rogue:945492995480883210>", SearchTerms = "rogue;rouge" },
        new() { Id = 07, Name = "Shaman", Icon = "<:shaman:945492995459940402>", SearchTerms = "shaman;shammy;sham" },
        new() { Id = 08, Name = "Warlock", Icon = "<:warlock:945492995648671754>", SearchTerms = "warlock;lock" },
        new() { Id = 09, Name = "Warrior", Icon = "<:warrior:945492995602538546>", SearchTerms = "warrior;warr" },
        new() { Id = 10, Name = "Death Knight", Icon = "<:deathknight:945492994964979722>", SearchTerms = "death knight;deathknight;dk" },
        new() { Id = 11, Name = "Monk", Icon = "<:monk:945492996554629170>", SearchTerms = "monk" },
        new() { Id = 12, Name = "Demon Hunter", Icon = "<:demonhunter:945492995149545552>", SearchTerms = "demon hunter;demonhunter;dh" },
        new() { Id = 13, Name = "Evoker", Icon = "<:evoker:1179290186530697296>", SearchTerms = "evoker" },
    ];

    private readonly List<Expansion> _expansions = [];

    private readonly List<ExpansionClass> _expansionClasses = [];

    private void AddClass(int expansionId, int classId)
    {
        _expansionClasses.Add(new() { ClassId = classId, ExpansionId = expansionId });
    }

    private void AddExpansion(int id, string name, string shortName, IncludeClasses includeClasses)
    {
        _expansions.Add(new() { Name = name, ShortName = shortName, Id = id });

        AddClass(id, 01); // druid
        AddClass(id, 02); // hunter
        AddClass(id, 03); // mage

        if (includeClasses != IncludeClasses.ClassicHorde)
        {
            AddClass(id, 04); // paladin
        }

        AddClass(id, 05); // priest
        AddClass(id, 06); // rogue

        if (includeClasses != IncludeClasses.ClassicAlliance)
        {
            AddClass(id, 07); // shaman
        }

        AddClass(id, 08); // warlock
        AddClass(id, 09); // warrior

        if (includeClasses >= IncludeClasses.Wrath)
        {
            AddClass(id, 10); // death knight

            if (includeClasses >= IncludeClasses.Mop)
            {
                AddClass(id, 11); // monk

                if (includeClasses >= IncludeClasses.Legion)
                {
                    AddClass(id, 12); // demon hunter

                    if (includeClasses >= IncludeClasses.Dragonflight)
                    {
                        AddClass(id, 13); // evoker
                    }
                }
            }
        }
    }

    public void Build(out Expansion[] expansions, out PlayerClass[] classes, out ExpansionClass[] expansionClasses)
    {
        AddExpansion(02, "The Burning Crusade", "TBC", IncludeClasses.Classic);
        AddExpansion(03, "Wrath of the Lich King", "WotLK", IncludeClasses.Wrath);
        AddExpansion(04, "Cataclysm", "Cata", IncludeClasses.Wrath);
        AddExpansion(05, "Mists of Pandaria", "Mists", IncludeClasses.Mop);
        AddExpansion(06, "Warlords of Draenor", "WoD", IncludeClasses.Mop);
        AddExpansion(07, "Legion", "Legion", IncludeClasses.Legion);
        AddExpansion(08, "Battle for Azeroth", "BfA", IncludeClasses.Legion);
        AddExpansion(09, "Shadowlands", "SL", IncludeClasses.Legion);
        AddExpansion(10, "Dragonflight", "DF", IncludeClasses.Dragonflight);
        //AddExpansion(11, "The War Within", "TWW", IncludeClasses.Dragonflight);
        //AddExpansion(12, "Midnight", "Midnight", IncludeClasses.Dragonflight);
        //AddExpansion(13, "The Last Titan", "TLT", IncludeClasses.Dragonflight);

        AddExpansion(101, "Classic (Alliance)", "Classic", IncludeClasses.ClassicAlliance);
        AddExpansion(102, "Classic (Horde)", "Classic", IncludeClasses.ClassicHorde);

        AddExpansion(111, "Season of Discovery (Alliance)", "SoD", IncludeClasses.ClassicAlliance);
        AddExpansion(112, "Season of Discovery (Horde)", "SoD", IncludeClasses.ClassicHorde);

        expansions = [.. _expansions];
        classes = _classes;
        expansionClasses = [.. _expansionClasses];
    }

    private enum IncludeClasses
    {
        None = 0,
        ClassicAlliance = 1,
        ClassicHorde = 2,
        Classic = 3,
        Wrath = 4,
        Mop = 5,
        Legion = 6,
        Dragonflight = 7
    }
}
