using System;

public static class NameGenerator
{
    private static Random _random = new();

    public static string New => $"{Adjectives[_random.Next(Adjectives.Length)]} {Nouns[_random.Next(Nouns.Length)]}";

    public static readonly string[] Adjectives =
    {
        "Brave", "Clever", "Swift", "Quiet", "Loud", "Gentle", "Wild", "Calm", "Bright", "Dark",
        "Golden", "Silver", "Crimson", "Azure", "Scarlet", "Emerald", "Amber", "Frosty", "Sunny", "Stormy",
        "Happy", "Sleepy", "Mighty", "Tiny", "Massive", "Nimble", "Shy", "Fierce", "Sneaky", "Daring",
        "Lucky", "Curious", "Playful", "Kind", "Bold", "Silent", "Swift", "Cunning", "Loyal", "Fearless",
        "Ancient", "Mystic", "Radiant", "Glowing", "Vivid", "Shadowed", "Bright", "Dusky", "Majestic", "Noble",
        "Spotted", "Striped", "Fluffy", "Rusty", "Icy", "Burning", "Shimmering", "Gleaming", "Sparkling", "Dusty",
        "Gentle", "Proud", "Cheerful", "Grumpy", "Lazy", "Eager", "Witty", "Drowsy", "Brilliant", "Charming",
        "Silly", "Gloomy", "Whispering", "Cackling", "Floating", "Hidden", "Wandering", "Flickering", "Shining", "Fuzzy",
        "Sturdy", "Courageous", "Dizzy", "Mysterious", "Enigmatic", "Peaceful", "Radiant", "Boundless", "Vast", "Tiny",
        "Chill", "Toasty", "Lucky", "Noble", "Shadowy", "Swiftfooted", "Iron", "Bronze", "Golden", "Silvered",
        "Jagged", "Smooth", "Misty", "Echoing", "Whispering", "Gilded", "Arcane", "Fabled", "Dawnlit", "Moonlit"
    };

    public static readonly string[] Nouns =
    {
        "Puppy", "Kitten", "Fox", "Wolf", "Bear", "Hawk", "Raven", "Owl", "Eagle", "Falcon",
        "Lion", "Tiger", "Panther", "Cheetah", "Jaguar", "Dog", "Cat", "Mouse", "Otter", "Ferret",
        "Penguin", "Seal", "Whale", "Shark", "Dolphin", "Turtle", "Crab", "Lobster", "Octopus", "Squid",
        "Deer", "Elk", "Moose", "Bison", "Horse", "Zebra", "Cow", "Sheep", "Goat", "Pig",
        "Oak", "Willow", "Birch", "Cedar", "Maple", "Aspen", "Pine", "Redwood", "Spruce", "Palm",
        "River", "Stream", "Ocean", "Wave", "Cloud", "Storm", "Mountain", "Valley", "Canyon", "Prairie",
        "Sun", "Moon", "Star", "Comet", "Planet", "Meteor", "Galaxy", "Sky", "Aurora", "Eclipse",
        "Flame", "Ember", "Ash", "Stone", "Rock", "Crystal", "Gem", "Shadow", "Echo", "Light",
        "Leaf", "Flower", "Rose", "Lily", "Violet", "Daisy", "Tulip", "Fern", "Moss", "Thorn",
        "Dragon", "Phoenix", "Unicorn", "Griffin", "Sprite", "Spirit", "Golem", "Wisp", "Imp", "Ghost",
        "Traveler", "Nomad", "Wanderer", "Guardian", "Seeker", "Hunter", "Sailor", "Scholar", "Knight", "Rogue",
        "Foxglove", "Pebble", "Candle", "Mirror", "Lantern", "Clock", "Bell", "Feather", "Shell", "Seed",
        "Falcon",
    };
}