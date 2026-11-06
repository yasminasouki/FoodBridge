namespace FoodBridge.Domain.Entities;

public sealed class ListingCategory
{
    public int ListingId { get; private set; }
    public int CategoryId { get; private set; }

    private ListingCategory()
    {
    }

    public ListingCategory(int listingId, int categoryId)
    {
        if (listingId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(listingId));
        }

        if (categoryId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(categoryId));
        }

        ListingId = listingId;
        CategoryId = categoryId;
    }
}
