namespace Miniblog.Web;

using System.Diagnostics.CodeAnalysis;

public static class Constants
{
    public const string AllCats = "AllCats";
    public const string AllTags = "AllTags";
    public const string Categories = "categories";
    public const string Tags = "tags";
    public const string Dash = "-";
    public const string Description = "Description";
    public const string Head = "Head";
    public const string Next = "next";
    public const string Page = "page";
    public const string Preload = "Preload";
    public const string Prev = "prev";
    public const string ReturnUrl = "ReturnUrl";
    public const string Scripts = "Scripts";
    public const string Slug = "slug";
    public const string Space = " ";
    public const string Title = "Title";
    public const string TotalPostCount = "TotalPostCount";
    public const string ViewOption = "ViewOption";

    [SuppressMessage(
        "Design",
        "CA1034:Nested types should not be visible",
        Justification = "Constant classes are nested for easy intellisense.")]
    public static class Config
    {
        public static class Blog
        {
            public const string Name = "blog:name";
        }

        public static class User
        {
            public const string Password = "user:password";
            public const string Salt = "user:salt";
            public const string UserName = "user:username";
        }
    }
}
