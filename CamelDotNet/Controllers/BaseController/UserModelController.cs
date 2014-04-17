using CamelDotNet.Lib;
using CamelDotNet.Models;
using CamelDotNet.Models.Base;
using CamelDotNet.Models.DAL;
using CamelDotNet.Models.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CamelDotNet.Controllers
{
    public class UserModelController<Model> : Controller where Model : CamelDotNetUser
    {
        public UnitOfWork UW;
        public GenericRepository<Model> GR;
        public List<string> Path;
        public string ViewPath1 = "~/Views/";
        public string ViewPath = "UserProfile";
        public string ViewPathBase = "UserProfile";
        public string ViewPath2 = "/";

        public UserModelController()
        {
            UW = new UnitOfWork();
            GR = (GenericRepository<Model>)(typeof(UnitOfWork).GetProperty(typeof(Model).Name + "Repository").GetValue(UW));
        }

        public virtual ActionResult Index(int page = 1, bool includeSoftDeleted = false, string filter = null)
        {
            ViewBag.RV = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", "Index" }, { "actionAjax", "Get" }, { "page", page }, { "includeSoftDeleted", includeSoftDeleted }, { "filter", filter } };
            return View(ViewPath1 + ViewPath + ViewPath2 + "Index.cshtml");
        }

        public virtual PartialViewResult Get(string returnRoot, string actionAjax = "", int page = 1, bool includeSoftDeleted = false, string filter = null)
        {
            var results = UserCommon<Model>.GetQuery(UW, includeSoftDeleted, filter);

            if (!includeSoftDeleted)
            {
                results = results.Where(a => a.IsDeleted == false);
            }

            results = results.OrderBy(a => a.UserName);

            var rv = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", returnRoot }, { "actionAjax", actionAjax }, { "page", page }, { "includeSoftDeleted", includeSoftDeleted }, { "filter", filter } };
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Get.cshtml", Common<Model>.Page(this, rv, results));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Details(string id = null, string returnUrl = "Index")
        {
            var result = UserCommon<Model>.GetQuery(UW).Where(a => a.Id == id).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Details.cshtml", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(string returnUrl = "Index")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(ViewPath1 + ViewPath + ViewPath2 + "Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult CreateSave(RegisterViewModel model, string returnUrl = "Index")
        {
            var UserManager = new UserManager<CamelDotNetUser>(new UserStore<CamelDotNetUser>(UW.context));
            //允许用户名包含非字母、数字
            UserManager.UserValidator = new UserValidator<CamelDotNetUser>(UserManager) { AllowOnlyAlphanumericUserNames = false};

            if (ModelState.IsValid)
            {
                try
                {
                    var user = new CamelDotNetUser();
                    user.UserName = model.UserName;
                    user.JobNumber = model.JobNumber;
                    user.CamelDotNetRoleId = model.CamelDotNetRoleId;
                    user.DepartmentId = model.DepartmentId;
                    var userResult = UserManager.Create(user, model.Password);
                    UW.CamelUserSave();

                    Common.RMOk(this, "记录:" + model.UserName + "新建成功!");
                    return Redirect(Url.Content(returnUrl));
                }
                catch (UpdateException e)
                {
                    if (e.InnerException.Message.Contains("Cannot insert duplicate key row"))
                    {
                        ModelState.AddModelError(string.Empty, "相同名称的记录已存在,保存失败!");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "新建记录失败!" + e.ToString());
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, "新建记录失败!" + e.ToString());
                }
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(ViewPath1 + ViewPath + ViewPath2 + "Create.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(string id = null, string returnUrl = "Index")
        {
            var result = UserCommon<Model>.GetQuery(UW).Where(a => a.Id == id).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", result);
        }

        //
        // POST: /Model/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult EditSave(Model model, string returnUrl = "Index")
        {
            //检查记录在权限范围内
            var result = UserCommon<Model>.GetQuery(UW).Where(a => a.Id == model.Id).SingleOrDefault();
            if (result == null)
            {
                //Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }
            //end 检查记录在权限范围内

            if (ModelState.IsValid)
            {
                try
                {
                    result.Edit(model);
                    UW.CamelSave();
                    Common.RMOk(this, "记录:" + result + "保存成功!");
                    return Redirect(Url.Content(returnUrl));
                }
                catch (UpdateException e)
                {
                    if (e.InnerException.Message.Contains("Cannot insert duplicate key row"))
                    {
                        ModelState.AddModelError(string.Empty, "相同名称的记录已存在,保存失败!");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "编辑记录失败!" + e.ToString());
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, "编辑记录失败!" + e.ToString());
                }
            }
            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Delete(string id = null, string returnUrl = "Index")
        {
            //检查记录在权限范围内
            var result = UserCommon<Model>.GetQuery(UW).Where(a => a.Id == id).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }
            //end 检查记录在权限范围内

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Delete.cshtml", result);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult DeleteSave(string id, string returnUrl = "Index")
        {
            //检查记录在权限范围内
            var result = UserCommon<Model>.GetQuery(UW).Where(a => a.Id == id).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }
            //end 检查记录在权限范围内
            var removeName = result.ToString();
            try
            {
                GR.Delete(id);
                UW.CamelUserSave();
                Common.RMOk(this, "记录:" + removeName + "删除成功!");
                return Redirect(Url.Content(returnUrl));
            }
            catch (Exception e)
            {
                Common.RMError(this, "记录" + removeName + "删除失败!" + e.ToString());
            }
            return Redirect(Url.Content(returnUrl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Restore(string id = null, string returnUrl = "Index")
        {
            //检查记录在权限范围内
            var result = UserCommon<Model>.GetQuery(UW, true).Where(a => a.Id == id && a.IsDeleted == true).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }
            //end 检查记录在权限范围内

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Restore.cshtml", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult RestoreSave(Model model, string returnUrl = "Index")
        {
            var result = UserCommon<Model>.GetQuery(UW, true).Where(a => a.Id == model.Id && a.IsDeleted == true).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            try
            {
                result.IsDeleted = false;
                UW.CamelSave();
                Common.RMOk(this, "记录:" + result + "恢复成功!");
                return Redirect(Url.Content(returnUrl));
            }
            catch (Exception e)
            {
                Common.RMOk(this, "记录" + result + "恢复失败!" + e.ToString());
            }
            return Redirect(Url.Content(returnUrl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string id, string returnUrl = "Index")
        {
            var UserManager = new UserManager<CamelDotNetUser>(new UserStore<CamelDotNetUser>(UW.context));
            var result = UserManager.FindById(id);
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            ResetPasswordModel model = new ResetPasswordModel() { Id = result.Id, UserName = result.UserName };

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "ResetPassword.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPasswordSave(ResetPasswordModel model,string returnUrl = "Index")
        {
            var UserManager = new UserManager<CamelDotNetUser>(new UserStore<CamelDotNetUser>(UW.context));

            if (ModelState.IsValid)
            {
                try
                {
                    CamelDotNetUser user = UserManager.FindById(model.Id);
                    string HashNewPassword = UserManager.PasswordHasher.HashPassword(model.NewPassword);
                    UserStore<CamelDotNetUser> store = new UserStore<CamelDotNetUser>(new CamelDotNetDBContext());
                    store.SetPasswordHashAsync(user, HashNewPassword);
                    store.UpdateAsync(user);
                    //UserManager.ChangePassword<CamelDotNetUser>(user.Id,user.PasswordHash,HashNewPassword);
                    //UserManager.Update<CamelDotNetUser>(user);
                    UW.CamelUserSave();

                    Common.RMOk(this, "记录:" + user.UserName + "重置密码成功！");
                    return Redirect(Url.Content(returnUrl));
                }
                catch (UpdateException e)
                {
                    if (e.InnerException.Message.Contains("Cannot insert duplicate key row"))
                    {
                        ModelState.AddModelError(string.Empty, "相同名称的记录已存在,保存失败!");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "新建记录失败!" + e.ToString());
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, "新建记录失败!" + e.ToString());
                }
            }
            return View(ViewPath1 + ViewPath + ViewPath2 + "ResetPassword.cshtml", model);
        }

        [ChildActionOnly]
        public virtual PartialViewResult Abstract(string id)
        {
            var result = GR.GetByID(id);
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Abstract.cshtml", result);
        }

        [ChildActionOnly]
        public virtual PartialViewResult AbstractEdit(string id)
        {
            var result = GR.GetByID(id);
            return PartialView(ViewPath1 + ViewPathBase + ViewPath2 + "AbstractEdit.cshtml", result);
        }

        protected override void Dispose(bool disposing)
        {
            UW.Dispose();
            base.Dispose(disposing);
        }
    }
}