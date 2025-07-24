using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    //public class BlogViewModel
    //{
    //    public BlogViewModel()
    //    {

    //        Post = new BlogPost();
    //        Category = new BlogCategory();
    //        Tags = new List<BlogTag>();
    //    }

    //    public BlogPost Post { get; set; }
    //    public BlogCategory Category { get; set; }
    //    public List<BlogTag> Tags { get; set; }
    //}

    public class BlogPostViewModel
    {
        public string PostId { get; set; }
        public string Title { get; set; }
        public string Lead { get; set; }
        public string AuthorName { get; set; }
        public string PublishedDate { get; set; }
        public int Status { get; set; }
        public string CategoryTitle { get; set; }
        public List<string> TagTitles { get; set; }
    }
}