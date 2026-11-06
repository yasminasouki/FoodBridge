namespace FoodBridge.Application.Security;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Shop = "Shop";
    public const string Receiver = "Receiver";
    public const string Ngo = "Ngo";

    public static readonly string[] All = [Admin, Shop, Receiver, Ngo];
}
