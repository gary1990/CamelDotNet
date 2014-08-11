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
using System.Globalization;
using System.Data.Entity;
using System.ComponentModel;
using System.Text.RegularExpressions;

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
                Expression right = null;
                foreach (var prop in propertyNames)
                {
                    left = Expression.PropertyOrField(left, prop);
                    
                    var type = left.Type.Name;
                    var typeFullName = left.Type.FullName;

                    if (type == "Int32")
                    {
                        right = Expression.Constant(Int32.Parse(target));
                    }
                    else if(type == "DateTime")
                    {
                        DateTime targetParse;
                        if (!DateTime.TryParseExact(target, "yyyyMMdd HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out targetParse)) 
                        {
                            targetParse = DateTime.Now;
                        }
                        right = Expression.Constant(targetParse);
                    }
                    else if (type == "Boolean")
                    {
                        Boolean targetBoolean = Convert.ToBoolean(target);
                        right = Expression.Constant(targetBoolean);
                    }
                    else if (type == "Nullable`1" && typeFullName.Contains("System.Int32"))
                    {
                        right = Expression.Constant(Int32.Parse(target));
                    }
                    else
                    {
                        right = Expression.Constant(target);
                    }
                }

                BinaryExpression call = null;
                if (tmp[1] == "=")
                {
                    call = MyEqual(left, right);
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

        static BinaryExpression MyEqual(Expression e1, Expression e2)
        {
            if (IsNullableType(e1.Type) && !IsNullableType(e2.Type))
                e2 = Expression.Convert(e2, e1.Type);
            else if (!IsNullableType(e1.Type) && IsNullableType(e2.Type))
                e1 = Expression.Convert(e1, e2.Type);
            return Expression.Equal(e1, e2);
        }
        static bool IsNullableType(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static IQueryable<Model> Page(Controller c, RouteValueDictionary rv, IQueryable<Model> q, int size = 20)
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

    public class UserCommon<Model> where Model : CamelDotNetUser 
    {
        public static IQueryable<Model> GetQuery(UnitOfWork db, bool includeSoftDeleted = false, string filter = null, bool noTrack = false)
        {
            IQueryable<Model> result;

            var rep = (GenericRepository<Model>)(typeof(UnitOfWork).GetProperty(typeof(Model).Name + "Repository").GetValue(db));

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
    }

    public class TestConfigCommon<Model> where Model : TestConfig
    {
        public static IQueryable<Model> GetQuery(UnitOfWork db, bool includeSoftDeleted = false, string filter = null, bool noTrack = false)
        {
            IQueryable<Model> result;

            var rep = (GenericRepository<Model>)(typeof(UnitOfWork).GetProperty(typeof(Model).Name + "Repository").GetValue(db));

            result = rep.Get(noTrack);

            if (!includeSoftDeleted)
            {
                result = result.Where(a => a.IsDeleted == false);
            }

            result = result.Where(a => a.Client.IsDeleted == false && a.ProductType.IsDeleted == false);

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
    }

    public class VnaRecordCommon<Model> where Model : VnaRecord
    {
        public static IQueryable<Model> GetQuery(UnitOfWork db, bool includeSoftDeleted = false, string filter = null, bool noTrack = false)
        {
            IQueryable<Model> result;

            var rep = (GenericRepository<Model>)(typeof(UnitOfWork).GetProperty(typeof(Model).Name + "Repository").GetValue(db));

            result = rep.Get(noTrack);

            result = result.Where(a => a.CamelDotNetUser.IsDeleted == false && a.ProductType.IsDeleted == false && a.TestEquipment.IsDeleted == false && 
                a.TestStation.IsDeleted == false);
            
            //filter
            if (filter != null)
            {
                Dictionary<string, string> filterDic = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    //reFormart filter if contains Time filter
                    filter = TimeFormat.TimeFilterConvert("TestTime", "TestTimeStartHour",">=",filter);
                    filter = TimeFormat.TimeFilterConvert("TestTime", "TestTimeStopHour", "<=", filter);
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
    }
    public class TimeFormat 
    {
        public static string TimeFilterConvert(string timeDateMark,string timeHourMark, string equalMark, string filterStr) 
        {
            //Target Time String yyyyMMdd HHmmss formarted
            var timeTarget = "";
            //Matched Time Date String
            var regDateStr = "";
            //Matched Time Hour String
            var regHourStr = "";
            //match Time Date
            Regex regDate = new Regex(@"(" + timeDateMark + @"\@" + equalMark + @"\:)([-\d]+)(;)");
            //match Time Hour
            Regex regHour = new Regex(@"(" + timeHourMark + @"\@" + equalMark + @"\:)([\d]+)(;)");
            //Time Date Match result
            bool regDateStartMatch = regDate.IsMatch(filterStr);
            //Time Hour Match result
            bool regHourStartMatch = regHour.IsMatch(filterStr);
            if (regDateStartMatch)
            {
                regDateStr = regDate.Match(filterStr).Value;
                //replace matched Time Date with space
                filterStr = regDate.Replace(filterStr, "");
                if (regHourStartMatch)
                {
                    regHourStr = regHour.Match(filterStr).Value;
                    //replace matched Time Hour with space
                    filterStr = regHour.Replace(filterStr, "");
                    //colon position in regHourStr
                    int colonPostion = regHourStr.IndexOf(':');
                    //generate Target Time String yyyyMMdd HHmmss formarted
                    timeTarget = regDateStr.Substring(0, regDateStr.Length - 1).Replace("-", "") + " " + regHourStr.Substring(colonPostion + 1, regHourStr.Length - colonPostion - 2) + "0000" + ";";
                }
                else
                {
                    //generate Target Time String yyyyMMdd HHmmss formarted
                    timeTarget = regDateStr.Substring(0, regDateStr.Length - 1).Replace("-", "") + " " + "000000" + ";";
                }
            }
            else
            {
                if (regHourStartMatch)
                {
                    regHourStr = regHour.Match(filterStr).Value;
                    //replace matched Time Hour with space
                    filterStr = regHour.Replace(filterStr, "");
                    //get current date String yyyyMMdd formated
                    string currentDateStr = DateTime.Now.ToString("yyyyMMdd");
                    //colon position in regHourStartStr
                    int colonPostion = regHourStr.IndexOf(':');
                    //generate Target Time String yyyyMMdd HHmmss formarted
                    timeTarget = @"TestTime@>=:" + currentDateStr + " " + regHourStr.Substring(colonPostion + 1, regHourStr.Length - colonPostion - 2) + "0000" + ";";
                }
                else
                {
                    //do noting
                }
            }
            //add Target Time String to filter String
            filterStr += timeTarget;
            return filterStr;
        }
    }
    public class QualityLossCommon<Model> where Model : QualityLoss
    {
        public static IQueryable<Model> GetQuery(UnitOfWork db, string filter = null, bool noTrack = false)
        {
            IQueryable<Model> result;

            var rep = (GenericRepository<Model>)(typeof(UnitOfWork).GetProperty(typeof(Model).Name + "Repository").GetValue(db));

            result = rep.Get(noTrack);

            result = result.Include(a => a.QualityLossPercents);

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
        public static List<CamelDotNetUser> GetCamelDotNetUserList(string keyWord = null)
        {
            using (var db = new UnitOfWork())
            {
                return GetCamelDotNetUserQuery(db, keyWord, true).ToList();
            }
        }
        public static IQueryable<CamelDotNetUser> GetCamelDotNetUserQuery(UnitOfWork db, string keyWord = null, bool noTrack = false)
        {
            IQueryable<CamelDotNetUser> result;

            var rep = db.CamelDotNetUserRepository;

            result = rep.Get(noTrack);

            result = result.Where(a => a.IsDeleted == false);

            if (!String.IsNullOrWhiteSpace(keyWord))
            {
                keyWord = keyWord.ToUpper();
                result = result.Where(a => a.UserName.ToUpper().Contains(keyWord));
            }

            return result;
        }
        public static List<TestStation> GetTestStationList(string keyWord = null)
        {
            using (var db = new UnitOfWork())
            {
                return GetTestStationQuery(db, keyWord, true).ToList();
            }
        }
        public static IQueryable<TestStation> GetTestStationQuery(UnitOfWork db, string keyWord = null, bool noTrack = false)
        {
            IQueryable<TestStation> result;

            var rep = db.TestStationRepository;

            result = rep.Get(noTrack);

            result = result.Where(a => a.IsDeleted == false);

            if (!String.IsNullOrWhiteSpace(keyWord))
            {
                keyWord = keyWord.ToUpper();
                result = result.Where(a => a.Name.ToUpper().Contains(keyWord));
            }

            return result;
        }

        public static List<TestItem> GetTestItemElecList(string keyWord = null)
        {
            using (var db = new UnitOfWork())
            {
                return GetTestItemElecQuery(db, keyWord, true).ToList();
            }
        }
        public static IQueryable<TestItem> GetTestItemElecQuery(UnitOfWork db, string keyWord = null, bool noTrack = false)
        {
            IQueryable<TestItem> result;

            var rep = db.TestItemRepository;

            result = rep.Get(noTrack);

            result = result.Where(a => a.IsDeleted == false && a.TestItemCategory.Name == "电气性能");

            if (!String.IsNullOrWhiteSpace(keyWord))
            {
                keyWord = keyWord.ToUpper();
                result = result.Where(a => a.Name.ToUpper().Contains(keyWord));
            }

            return result;
        }

        public static List<TestItem> GetTestItemList(string keyWord = null)
        {
            using (var db = new UnitOfWork())
            {
                return GetTestItemQuery(db, keyWord, true).ToList();
            }
        }
        public static IQueryable<TestItem> GetTestItemQuery(UnitOfWork db, string keyWord = null, bool noTrack = false)
        {
            IQueryable<TestItem> result;

            var rep = db.TestItemRepository;

            result = rep.Get(noTrack);

            result = result.Where(a => a.IsDeleted == false);

            if (!String.IsNullOrWhiteSpace(keyWord))
            {
                keyWord = keyWord.ToUpper();
                result = result.Where(a => a.Name.ToUpper().Contains(keyWord));
            }

            return result;
        }

        public static List<ProductType> GetProductByIdList(int productTypeId) 
        {
            using (var db = new UnitOfWork())
            {
                return GetProductByIdQury(db, productTypeId).ToList();
            }
        }
        public static IQueryable<ProductType> GetProductByIdQury(UnitOfWork db, int productTypeId)
        {
            var result = db.context.ProductType.Where(a => a.Id == productTypeId);
            return result;
        }

        public static List<ProductType> GetProductTypeInTestConfigList(string keyWord = null)
        {
            using (var db = new UnitOfWork())
            {
                return GetProductTypeInTestConfigQuery(db, keyWord, true).ToList();
            }
        }
        public static IQueryable<ProductType> GetProductTypeInTestConfigQuery(UnitOfWork db, string keyWord = null, bool noTrack = false)
        {
            var result = db.context.TestConfig.Select(a => a.ProductType).Distinct();
            return result;
        }

        public static List<ProductType> GetProductTypeList(string keyWord = null)
        {
            using (var db = new UnitOfWork())
            {
                return GetProductTypeQuery(db, keyWord, true).ToList();
            }
        }
        public static IQueryable<ProductType> GetProductTypeQuery(UnitOfWork db, string keyWord = null, bool noTrack = false)
        {
            IQueryable<ProductType> result;

            var rep = db.ProductTypeRepository;

            result = rep.Get(noTrack);

            result = result.Where(a => a.IsDeleted == false);

            if (!String.IsNullOrWhiteSpace(keyWord))
            {
                keyWord = keyWord.ToUpper();
                result = result.Where(a => a.Name.ToUpper().Contains(keyWord));
            }

            return result;
        }
        public static List<Client> GetClientList(string keyWord = null)
        {
            using (var db = new UnitOfWork())
            {
                return GetClientQuery(db, keyWord, true).ToList();
            }
        }
        public static IQueryable<Client> GetClientQuery(UnitOfWork db, string keyWord = null, bool noTrack = false)
        {
            IQueryable<Client> result;

            var rep = db.ClientRepository;

            result = rep.Get(noTrack);

            result = result.Where(a => a.IsDeleted == false);

            if (!String.IsNullOrWhiteSpace(keyWord))
            {
                keyWord = keyWord.ToUpper();
                result = result.Where(a => a.Name.ToUpper().Contains(keyWord));
            }

            return result;
        }

        public static List<TestItemCategory> GetTestItemCategoryList(string keyWord = null)
        {
            using (var db = new UnitOfWork())
            {
                return GetTestItemCategoryQuery(db, keyWord, true).ToList();
            }
        }
        public static IQueryable<TestItemCategory> GetTestItemCategoryQuery(UnitOfWork db, string keyWord = null, bool noTrack = false)
        {
            IQueryable<TestItemCategory> result;

            var rep = db.TestItemCategoryRepository;

            result = rep.Get(noTrack);

            if (!String.IsNullOrWhiteSpace(keyWord))
            {
                keyWord = keyWord.ToUpper();
                result = result.Where(a => a.Name.ToUpper().Contains(keyWord));
            }

            return result;
        }
        public static List<CamelDotNetRole> GetRoleList(string keyWord = null) 
        {
            using (var db = new UnitOfWork())
            {
                return GetRoleQuery(db, keyWord, true).ToList();
            }
        }

        public static IQueryable<CamelDotNetRole> GetRoleQuery(UnitOfWork db, string keyWord = null, bool noTrack = false) 
        {
            IQueryable<CamelDotNetRole> result;
            var rep = db.CamelDotNetRoleRepository;
            result = rep.Get(noTrack);
            if (!String.IsNullOrWhiteSpace(keyWord))
            {
                keyWord = keyWord.ToUpper();
                result = result.Where(a => a.Name.ToUpper().Contains(keyWord));
            }
            return result;
        }
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

        public static List<Unit> GetUnitList(string keyWord = null)
        {
            using (var db = new UnitOfWork())
            {
                return GetUnitQuery(db, keyWord, true).ToList();
            }
        }
        public static IQueryable<Unit> GetUnitQuery(UnitOfWork db, string keyWord = null, bool noTrack = false)
        {
            IQueryable<Unit> result;

            var rep = db.UnitRepository;

            result = rep.Get(noTrack);

            if (!String.IsNullOrWhiteSpace(keyWord))
            {
                keyWord = keyWord.ToUpper();
                result = result.Where(a => a.Name.ToUpper().Contains(keyWord));
            }

            return result;
        }

        public static List<Department> GetDepartmentList(string keyWord = null)
        {
            using (var db = new UnitOfWork())
            {
                return GetDepartmentQuery(db, keyWord, true).ToList();
            }
        }
        public static IQueryable<Department> GetDepartmentQuery(UnitOfWork db, string keyWord = null, bool noTrack = false)
        {
            IQueryable<Department> result;

            var rep = db.DepartmentRepository;

            result = rep.Get(noTrack);

            result = result.Where(a => a.IsDeleted == false);

            if (!String.IsNullOrWhiteSpace(keyWord))
            {
                keyWord = keyWord.ToUpper();
                result = result.Where(a => a.Name.ToUpper().Contains(keyWord));
            }

            return result;
        }
        //带消息提示的返回索引页面
        public static void RMError(Controller controller, string msg = "没有找到对应记录")
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