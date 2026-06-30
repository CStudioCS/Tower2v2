using UnityEngine;

public static class SessionNameGenerator
{
    private static readonly string[] Adjectives =
    {
        "Crazy", "Sad", "Happy", "Angry", "Sleepy", "Dizzy", "Brave", "Clumsy", "Charlie", "Shiny", "Sneaky", "Epic", "Golden", "Silver", "Mighty", "Tiny", "Giant", "Lazy", "Clever", "Silly", "Wild"
    };

    private static readonly string[] Nouns =
    {
        "Monkey", "Pizzo", "Dictator", "Thomas", "Jules", "Amazigh", "Farès", "Aurélie", "Pierre", "Cédric", "Nathanaël", "Mathis", "John", "Donkey", "Panda", "Turtle", "Tiger", "Penguin", "Chicken", "Wilson", "Kirk", "Raccoon", "Koala", "Otter", "Hamster"
    };

    public static string Generate()
    {
        string adj = Adjectives[Random.Range(0, Adjectives.Length)];
        string noun = Nouns[Random.Range(0, Nouns.Length)];
        int number = Random.Range(10, 99);

        return $"{adj}{noun}{number}";
    }
}