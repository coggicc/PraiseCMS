using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Net;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    [RequirePermission(ModuleId = "507474028267cbbbccfaf9475d8159")]
    public class BlogController : BaseController
    {
        #region Posts
        public ActionResult Index()
        {
            var posts = work.BlogPost.GetAllPosts();
            return View(posts);
        }

        public ActionResult CreateEditPost(string id = null)
        {
            var model = work.BlogPost.GetCreateEditModel(id);
            ViewBag.BlogCategories = work.BlogCategory.GetBlogCategoryDropdownList();
            return View("CreateEditPost", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEditPost(BlogPost model, string action)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Id))
                {
                    model.Id = Utilities.GenerateUniqueId();
                    model.CreatedDate = DateTime.Now;
                    model.CreatedBy = SessionVariables.CurrentUser.User.Id;
                }

                var result = action == "update" ? work.BlogPost.UpdatePost(model) : work.BlogPost.CreatePost(model);

                if (result.ResultType == ResultType.Success)
                {
                    CreateAlertMessage(String.Format("The blog post has been {0}.", action == "update" ? "updated" : "created"), AlertMessageTypes.Success, AlertMessageIcons.Success);
                    return RedirectToAction("index", "blog");
                }

                ViewBag.BlogCategories = work.BlogCategory.GetBlogCategoryDropdownList();
                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                return View("CreateEditPost", model);
            }
            else
            {
                ViewBag.BlogCategories = work.BlogCategory.GetBlogCategoryDropdownList();
                DisplayErrors();
                return View("CreateEditPost", model);
            }
        }

        //public ActionResult CreatePost()
        //{
        //    var model = new BlogPost
        //    {
        //        Id = Utilities.GenerateUniqueId(),
        //        CreatedDate = DateTime.Now,
        //        CreatedBy = SessionVariables.CurrentUser.User.Id
        //    };

        //    return PartialView("_CreateEditPost", model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult CreatePost(BlogPost model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var result = work.BlogPost.CreatePost(model);

        //        if (result.ResultType == ResultType.Success)
        //        {
        //            CreateAlertMessage("The blog post has been created.", AlertMessageTypes.Success, AlertMessageIcons.Success);
        //            return AjaxRedirectTo("/blog");
        //        }

        //        CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
        //    }

        //    return PartialView("_CreateEditPost", model);
        //}

        public ActionResult EditPost(string id)
        {
            var post = work.BlogPost.GetPost(id);
            ViewBag.BlogCategories = work.BlogCategory.GetBlogCategoryDropdownList();
            return PartialView("_CreateEditPost", post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(BlogPost model)
        {
            if (ModelState.IsValid)
            {
                var result = work.BlogPost.UpdatePost(model);

                if (result.ResultType == ResultType.Success)
                {
                    CreateAlertMessage("The blog post has been updated.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                    return AjaxRedirectTo("/blog");
                }

                ViewBag.BlogCategories = work.BlogCategory.GetBlogCategoryDropdownList();
                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            ViewBag.BlogCategories = work.BlogCategory.GetBlogCategoryDropdownList();
            return PartialView("_CreateEditPost", model);
        }

        [HttpGet]
        public ActionResult DeletePost(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var blogPost = work.BlogPost.GetPost(id);

            if (blogPost == null)
            {
                return HttpNotFound();
            }

            return View(blogPost);
        }

        // POST: BlogPost/Delete/5
        [HttpPost, ActionName("DeletePost")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var blogPost = work.BlogPost.GetPost(id);

            if (blogPost == null)
            {
                return HttpNotFound();
            }

            work.BlogPost.DeletePost(blogPost);
            CreateAlertMessage("The blog post has been deleted.", AlertMessageTypes.Success, AlertMessageIcons.Success);
            return RedirectToAction("index", "blog");
        }

        #endregion

        #region Categories
        public ActionResult Category()
        {
            return View();
        }

        public ActionResult Categories()
        {
            var categories = work.BlogCategory.GetAllCategories(false);
            return View(categories);
        }

        public ActionResult CreateCategory()
        {
            var model = new BlogCategory
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedDate = DateTime.Now,
                CreatedBy = SessionVariables.CurrentUser.User.Id
            };

            return PartialView("_CreateEditCategory", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(BlogCategory model)
        {
            if (ModelState.IsValid)
            {
                var result = work.BlogCategory.CreateCategory(model);

                if (result.ResultType == ResultType.Success)
                {
                    CreateAlertMessage("The blog category has been created.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                    return AjaxRedirectTo("/blog/categories");
                }

                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return PartialView("_CreateEditCategory", model);
        }

        public ActionResult EditCategory(string id)
        {
            var category = work.BlogCategory.GetCategory(id);
            return PartialView("_CreateEditCategory", category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(BlogCategory model)
        {
            if (ModelState.IsValid)
            {
                var result = work.BlogCategory.UpdateCategory(model);

                if (result.ResultType == ResultType.Success)
                {
                    CreateAlertMessage("The blog category has been updated.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                    return AjaxRedirectTo("/blog/categories");
                }

                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return PartialView("_CreateEditCategory", model);
        }

        public ActionResult DeleteCategory(string id)
        {
            var result = work.BlogCategory.DeleteCategory(id);

            if (result.ResultType == ResultType.Success)
            {
                CreateAlertMessage("The blog category has been deleted.", AlertMessageTypes.Success, AlertMessageIcons.Success);
            }
            else
            {
                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return RedirectToAction("/categories");
        }
        #endregion

        #region Tags
        public ActionResult Tag()
        {
            return View();
        }

        public ActionResult Tags()
        {
            var tags = work.BlogTag.GetAllTags(false);
            return View(tags);
        }

        public ActionResult CreateTag()
        {
            var model = new BlogTag
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedDate = DateTime.Now,
                CreatedBy = SessionVariables.CurrentUser.User.Id
            };

            return PartialView("_CreateEditTag", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTag(BlogTag model)
        {
            if (ModelState.IsValid)
            {
                var result = work.BlogTag.CreateTag(model);

                if (result.ResultType == ResultType.Success)
                {
                    CreateAlertMessage("The blog tag has been created.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                    return AjaxRedirectTo(nameof(Tags));
                }

                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return PartialView("_CreateEditTag", model);
        }

        public ActionResult EditTag(string id)
        {
            var tag = work.BlogTag.GetTag(id);
            return PartialView("_CreateEditTag", tag);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTag(BlogTag model)
        {
            if (ModelState.IsValid)
            {
                var result = work.BlogTag.UpdateTag(model);

                if (result.ResultType == ResultType.Success)
                {
                    CreateAlertMessage("The blog tag has been updated.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                    return AjaxRedirectTo(nameof(Tags));
                }

                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return PartialView("_CreateEditTag", model);
        }

        public ActionResult DeleteTag(string id)
        {
            var result = work.BlogTag.DeleteTag(id);

            if (result.ResultType == ResultType.Success)
            {
                CreateAlertMessage("The blog tag has been deleted.", AlertMessageTypes.Success, AlertMessageIcons.Success);
            }
            else
            {
                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return RedirectToAction(nameof(Tags));
        }
        #endregion
    }
}