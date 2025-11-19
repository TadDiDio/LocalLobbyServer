using System;

public static class NameGenerator
{
    private static Random _random = new();

    public static string New => $"{Adjectives[_random.Next(Adjectives.Length)]} {Nouns[_random.Next(Nouns.Length)]}";

   public static readonly string[] Adjectives =
    {
        "Moist", "Goopy", "Wiggly", "Wobbly", "Sussy", "Borb", "Glorp", "Spicy",
        "Crunchy", "Drippy", "Florp", "Chonky", "Feral", "Gremlin", "Brittle",
        "Goofy", "Wonky", "Derpy", "Zonky", "Gunky", "Boneless", "Extra", "Soggy",
        "Fluffy", "Squishy", "Thicc", "Unstable", "Rancid", "Stinky", "Melty",
        "Chaos", "Jiggly", "Frumpy", "Zappy", "Oddball", "Lumpy", "Krusty",
        "Yeet", "Smeary", "Dank", "Cursed", "Blessed", "Blobby"
    };

    public static readonly string[] Nouns =
    {
        "Gobbo", "Noodle", "Orb", "Frog", "Gourd", "Muffin", "Bean", "Blorb",
        "Gremlin", "Slug", "Biscuit", "Pog", "Turtle", "Worm", "Pebble", "Gnome",
        "Crouton", "Bun", "Chonk", "Guppy", "Boba", "Raccoon", "Puff", "Goo",
        "Sprout", "Pickle", "Mango", "Waffle", "Nugget", "Pigeon", "Spoon",
        "Ferret", "Goose", "Hamster", "Toad", "Muff", "Blob", "Gloop"
    };
}