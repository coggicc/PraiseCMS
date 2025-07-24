using PraiseCMS.Web.Controllers.Base;
using System.Web.Mvc;

namespace SalesWebsite.Controllers
{
    public class BlogController : BaseController
    {
        public ActionResult Index()
        {
            //var posts = work.BlogPost.GetAllPosts();
            var posts = work.BlogPost.GetAllActivePosts();
            return View(posts);
        }

        #region Posts
        public ActionResult Post(string id)
        {
            var post = work.BlogPost.GetPost(id);
            return View(post);
        }

        public ActionResult LatestPosts()
        {
            var posts = work.BlogPost.GetLatestPosts(4);
            return View(posts);
        }
        #endregion

        #region Categories
        public ActionResult Category(string id)
        {
            var category = work.BlogCategory.GetCategory(id);
            return View(category);
        }

        public ActionResult Categories()
        {
            var categories = work.BlogCategory.GetAllCategories(true);
            return View(categories);
        }
        #endregion

        #region Tags
        public ActionResult Tag(string id)
        {
            var tag = work.BlogTag.GetTag(id);
            return View(tag);
        }

        public ActionResult Tags()
        {
            var tags = work.BlogTag.GetAllTags(true);
            return View(tags);
        }
        #endregion

        [HttpGet]
        public ActionResult _BlogRightSidebar()
        {
            var tags = work.BlogTag.GetAllTags(true);

            //var latestPosts = work.BlogPost.GetLatestPosts(4);

            return PartialView("_BlogRightSidebar", tags);
        }
    }
}