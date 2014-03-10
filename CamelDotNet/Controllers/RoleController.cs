using CamelDotNet.Lib;
using CamelDotNet.Models;
using CamelDotNet.Models.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class RoleController : BaseModelController<CamelDotNetRole>
    {
        private CamelDotNetDBContext db = new CamelDotNetDBContext();

        List<string> path = new List<string>();
        public RoleController()
        {
            path.Add("系统管理");
            path.Add("角色管理");
            ViewBag.path = path;
            ViewBag.Name = "角色管理";
            ViewBag.Title = "角色管理";
            ViewBag.Controller = "Role";
            ViewPath = "Role";
        }
        public ActionResult EditSaveWithMultiselect(int id, FormCollection formCollection, string[] Name, string[] Items, string returnUrl = "Index")
        {
            string name = Name.FirstOrDefault();
            string[] selectedItem = Items;
            CamelDotNetRole currItem = db.CamelDotNetRole.Where(i => i.Id == id).Single();
            var existNameCount = db.CamelDotNetRole.Where(a => a.Name == name && a.Id != id).Count();
            if (existNameCount != 0)
            {
                ModelState.AddModelError(string.Empty, "相同名称的记录已存在,保存失败!");
            }
            else 
            {
                if (TryUpdateModel(currItem, "", null, new string[] { "Permissions" }))
                {
                    try 
                    {
                        UpdateCurrItem(selectedItem,currItem);

                        db.Entry(currItem).State = EntityState.Modified;
                        db.SaveChanges();
                        Common.RMOk(this, "记录:" + currItem + "保存成功!");
                        return Redirect(Url.Content(returnUrl));
                    }
                    catch (DataException) 
                    {
                        ModelState.AddModelError(string.Empty, "记录保存失败，请重试。");
                    }  
                }
            }

            return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", currItem);
        }
        
        public MultiSelectList GetRelatedList(CamelDotNetRole role) 
        {
            return new MultiSelectList(db.Permission, "Id", "Name", role.Permissions.Select(a => a.Id));
        }

        private void UpdateCurrItem(string[] selectedItem, CamelDotNetRole currItem) 
        {
            if(selectedItem == null)
            {
                currItem.Permissions = new List<Permission>();
                return;
            }

            var selectedItemHS = new HashSet<string>(selectedItem);
            var subItems = new HashSet<int>(currItem.Permissions.Select(c => c.Id));

            foreach(var p in db.Permission)
            {
                if(selectedItemHS.Contains(p.Id.ToString()))
                {
                    if(!subItems.Contains(p.Id))
                    {
                        currItem.Permissions.Add(p);
                    }
                }
                else
                {
                    if (subItems.Contains(p.Id))
                    {
                        currItem.Permissions.Remove(p);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}