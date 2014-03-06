using CamelDotNet.Models.Base;
using CamelDotNet.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using CamelDotNet.Models.ViewModels;
using CamelDotNet.Models;
namespace CamelDotNet.Lib
{
    public class Common<Model> where Model : class
    {
        public static IQueryable<Model> DynamicContains<TProperty>(
            IQueryable<Model> query,
            string property,
            IEnumerable<TProperty> items)
        {
            var pe = Expression.Parameter(typeof(Model));
            var me = Expression.Property(pe, property);

            var nullExpression = Expression.Constant(null);
            var call1 = Expression.Equal(me, nullExpression);

            var ce = Expression.Constant(items);
            var call2 = Expression.Call(typeof(Enumerable), "Contains", new[] { me.Type }, ce, me);

            var call = Expression.OrElse(call1, call2);

            var lambda = Expression.Lambda<Func<Model, bool>>(call, pe);
            return query.Where(lambda);
        }

        //property: xxxx@=, xxxx@>, xxxx@>= ... 
        public static IQueryable<Model> DynamicFilter(
            IQueryable<Model> query,
            string property,
            string target)
        {
            var pe = Expression.Parameter(typeof(Model), "pe");

            var tmp = property.Split('@');

            if (tmp[1] == "%")//模糊匹配
            {
                var words = tmp[0].Split('|');

                MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                var wordsExps = new List<Expression>();
                foreach (var item in words)
                {
                    var propertyNames = item.Split('.');
                    Expression left = pe;

                    foreach (var prop in propertyNames)
                    {
                        left = Expression.PropertyOrField(left, prop);
                    }

                    Expression upperLeft = Expression.Call(left, typeof(string).GetMethod("ToUpper", System.Type.EmptyTypes));

                    var upperRight = Expression.Constant(target.ToUpper());

                    wordsExps.Add(Expression.Call(upperLeft, method, upperRight));
                }

                Expression finalExp = wordsExps[0];
                for (int i = 1; i < wordsExps.Count; i++)
                {
                    finalExp = Expression.OrElse(finalExp, wordsExps[i]);
                }
                var lambda = Expression.Lambda<Func<Model, bool>>(finalExp, pe);
                return query.Where(lambda);
            }
            else
            {

                var propertyNames = tmp[0].Split('.');

                Expression left = pe;
                foreach (var prop in propertyNames)
                {
                    left = Expression.PropertyOrField(left, prop);
                }

                var right = Expression.Constant(target);

                BinaryExpression call = null;
                if (tmp[1] == "=")
                {
                    call = Expression.Equal(left, right);
                }
                else if (tmp[1] == ">")
                {
                    call = Expression.GreaterThan(left, right);
                }
                else if (tmp[1] == ">=")
                {
                    call = Expression.GreaterThanOrEqual(left, right);
                }
                else if (tmp[1] == "<")
                {
                    call = Expression.LessThan(left, right);
                }
                else if (tmp[1] == "<=")
                {
                    call = Expression.LessThanOrEqual(left, right);
                }

                var lambda = Expression.Lambda<Func<Model, bool>>(call, pe);
                return query.Where(lambda);
            }

        }

        public static IQueryable<Model> Page(Controller c, RouteValueDictionary rv, IQueryable<Model> q, int size = 2)
        {
            var tmpPage = rv.Where(a => a.Key == "page").Select(a => a.Value).SingleOrDefault();
            int page = int.Parse(tmpPage.ToString());
            var tmpTotalPage = (int)Math.Ceiling(((decimal)(q.Count()) / size));
            page = page > tmpTotalPage ? tmpTotalPage : page;
            page = page == 0 ? 1 : page;
            rv.Add("totalPage", tmpTotalPage);
            rv["page"] = page;

            c.ViewBag.RV = rv;
            return q.Skip(((tmpTotalPage > 0 ? page : 1) - 1) * size).Take(size);
        }
    }

    public class BaseCommon<Model> where Model : BaseModel
    {
        //query and list
        public static List<Model> GetList(bool includeSoftDeleted = false, string filter = null)
        {
            using (var db = new UnitOfWork())
            {
                return GetQuery(db, includeSoftDeleted, filter, true).ToList();
            }
        }
        public static IQueryable<Model> GetQuery(UnitOfWork db, bool includeSoftDeleted = false, string filter = null, bool noTrack = false)
        {
            IQueryable<Model> result;

            var rep = (GenericRepository<Model>)(typeof(UnitOfWork).GetProperty(typeof(Model).Name + "Repository").GetValue(db));

            List<string> typeNames = new List<string>();

            var baseType = typeof(Model).BaseType;
            var baseTypeName = baseType.Name;
            typeNames.Add(baseTypeName);

            while (baseTypeName != "BaseModel")
            {
                baseType = baseType.BaseType;
                baseTypeName = baseType.Name;
                typeNames.Add(baseTypeName);
            }

            result = rep.Get(noTrack);

            if (!includeSoftDeleted)
            {
                result = result.Where(a => a.IsDeleted == false);
            }

            //filter
            if (filter != null)
            {
                Dictionary<string, string> filterDic = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var conditions = filter.Substring(0, filter.Length - 1).Split(';');
                    foreach (var item in conditions)
                    {
                        var tmp = item.Split(':');
                        if (!string.IsNullOrWhiteSpace(tmp[1]))
                        {
                            filterDic.Add(tmp[0], tmp[1]);
                        }
                    }
                }

                foreach (var item in filterDic)
                {
                    result = Common<Model>.DynamicFilter(result, item.Key, item.Value);
                }
            }
            //end filter
            return result;
        }

        public static List<Model> GetClientList(int clientId, bool includeSoftDeleted = false, string filter = null)
        {
            using (var db = new UnitOfWork())
            {
                return GetClientQuery(db, clientId, includeSoftDeleted, filter, true).ToList();
            }
        }
        public static IQueryable<Model> GetClientQuery(UnitOfWork db, int clientId, bool includeSoftDeleted = false, string filter = null, bool noTrack = false)
        {
            filter = "ClientId@=" + clientId + ";" + filter;
            return GetQuery(db, includeSoftDeleted, filter, noTrack);
        }
    }

    public class Common
    {
        public static List<Process> GetProcessList(string keyWord = null) 
        {
            using(var db = new UnitOfWork())
            {
                return GetProcessQuery(db, keyWord, true).ToList();
            }
        }
        public static IQueryable<Process> GetProcessQuery(UnitOfWork db, string keyWord = null, bool noTrack = false)
        {
            IQueryable<Process> result;

            var rep = db.ProcessRepository;

            result = rep.Get(noTrack);

            if (!String.IsNullOrWhiteSpace(keyWord))
            {
                keyWord = keyWord.ToUpper();
                result = result.Where(a => a.Name.ToUpper().Contains(keyWord));
            }

            return result;
        }
        //带消息提示的返回索引页面
        public static void RMError(Controller controller, string msg = "权限范围内没有找到对应记录")
        {
            Msg message = new Msg { MsgType = MsgType.ERROR, Content = msg };
            controller.TempData["msg"] = message;
        }

        public static void RMOk(Controller controller, string msg = "操作成功!")
        {
            Msg message = new Msg { MsgType = MsgType.OK, Content = msg };
            controller.TempData["msg"] = message;
        }

        public static void RMWarn(Controller controller, string msg)
        {
            Msg message = new Msg { MsgType = MsgType.WARN, Content = msg };
            controller.TempData["msg"] = message;
        }
    }
}