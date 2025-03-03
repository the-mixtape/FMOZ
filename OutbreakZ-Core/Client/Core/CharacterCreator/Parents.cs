using System.Collections.Generic;

namespace OutbreakZCore.Client.Core.CharacterCreator
{
    public class ParentData
    {
        public string Name { get; set; }
        public int Index { get; set; }

        public ParentData(string name, int index)
        {
            Name = name;
            Index = index;
        }
    }

    public static class Parents
    {
        public static readonly List<ParentData> Moms = new List<ParentData>()
        {
            new ParentData("Hannah", 21),
            new ParentData("Audrey", 22),
            new ParentData("Jasmine", 23),
            new ParentData("Giselle", 24),
            new ParentData("Amelia", 25),
            new ParentData("Isabella", 26),
            new ParentData("Zoe", 27),
            new ParentData("Ava", 28),
            new ParentData("Camilla", 29),
            new ParentData("Violet", 30),
            new ParentData("Sophia", 31),
            new ParentData("Eveline", 32),
            new ParentData("Nicole", 33),
            new ParentData("Ashley", 34),
            new ParentData("Grace", 35),
            new ParentData("Brianna", 36),
            new ParentData("Natalie", 37),
            new ParentData("Olivia", 38),
            new ParentData("Elizabeth", 39),
            new ParentData("Charlotte", 40),
            new ParentData("Emma", 41),
            new ParentData("Misty", 45),
        };

        public static readonly List<ParentData> Dads = new List<ParentData>()
        {
            new ParentData("Benjamin", 0),
            new ParentData("Daniel", 1),
            new ParentData("Joshua", 2),
            new ParentData("Noah", 3),
            new ParentData("Andrew", 4),
            new ParentData("Joan", 5),
            new ParentData("Alex", 6),
            new ParentData("Isaac", 7),
            new ParentData("Evan", 8),
            new ParentData("Ethan", 9),
            new ParentData("Vincent", 10),
            new ParentData("Angel", 11),
            new ParentData("Diego", 12),
            new ParentData("Adrian", 13),
            new ParentData("Gabriel", 14),
            new ParentData("Michael", 15),
            new ParentData("Santiago", 16),
            new ParentData("Kevin", 17),
            new ParentData("Louis", 18),
            new ParentData("Samuel", 19),
            new ParentData("Anthony", 20),
            new ParentData("Claude", 44),
            new ParentData("Niko", 43),
            new ParentData("John", 42),
        };
    }
}