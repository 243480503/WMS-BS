/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_CodeGenApp
    {
        private IT_CodeGenRepository service = new T_CodeGenRepository();
        public IQueryable<T_CodeGenEntity> FindList(Expression<Func<T_CodeGenEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_CodeGenEntity FindEntity(Expression<Func<T_CodeGenEntity, bool>> predicate)
        {
            T_CodeGenEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public IQueryable<T_CodeGenEntity> FindListAsNoTracking(Expression<Func<T_CodeGenEntity, bool>> predicate)
        {
            return service.IQueryableAsNoTracking(predicate);
        }

        public T_CodeGenEntity FindEntityAsNoTracking(Expression<Func<T_CodeGenEntity, bool>> predicate)
        {
            T_CodeGenEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }

        public List<T_CodeGenEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public void Delete(Expression<Func<T_CodeGenEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }

        public List<T_CodeGenEntity> GetList(Pagination pagination, string keyword = "")
        {
            var expression = ExtLinq.True<T_CodeGenEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.RuleCode.Contains(keyword) || t.RuleName.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }
        public T_CodeGenEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_CodeGenEntity itemsEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(itemsEntity.Remark)) itemsEntity.Remark = itemsEntity.Remark.Replace("\n", " ");
            if (!string.IsNullOrEmpty(keyValue))
            {
                itemsEntity.Modify(keyValue);
                service.Update(itemsEntity);
            }
            else
            {
                itemsEntity.Create();
                service.Insert(itemsEntity);
            }
        }

        public void Insert(T_CodeGenEntity entity)
        {
            service.Insert(entity);
        }

        /// <summary>
        /// 获取所有单据或任务流水编码值
        /// </summary>
        /// <param name="ruleCode"></param>
        /// <returns></returns>
        public static string GenNum(string ruleCode)
        {
            IT_CodeGenRepository serviceTemp = new T_CodeGenRepository();
            var expression = ExtLinq.True<T_CodeGenEntity>();
            expression = expression.And(t => t.RuleCode == ruleCode);
            T_CodeGenEntity codeGen = serviceTemp.IQueryable(expression).ToList().FirstOrDefault();
            string format = codeGen.Format;
            DateTime now = DateTime.Now;
            format = format.Replace("[YYYY]", now.Year.ToString().PadLeft(4, '0'));
            format = format.Replace("[YY]", now.Year.ToString().Substring(2, 2));
            format = format.Replace("[MM]", now.Month.ToString().PadLeft(2, '0'));
            format = format.Replace("[DD]", now.Day.ToString().PadLeft(2, '0'));
            format = format.Replace("[H]", codeGen.FixStr.ToString());

            int lastGenData = Convert.ToInt32((codeGen.F_LastModifyTime ?? (Convert.ToDateTime("1990-01-01"))).ToString("yyyyMMdd"));
            int nowGenData = Convert.ToInt32(now.ToString("yyyyMMdd"));
            if (lastGenData == nowGenData)
            {
                codeGen.CurVal = codeGen.CurVal + 1;
            }
            else
            {
                codeGen.CurVal = 1;
            }

            string numcode = format.Replace("[NUM]", (codeGen.CurVal).ToString().PadLeft(codeGen.Length ?? 0, '0'));


            codeGen.Modify(codeGen.F_Id);
            serviceTemp.Update(codeGen);

            return numcode;
        }

        /// <summary>
        /// 获取所有单据或任务流水编码值(批量申请)
        /// </summary>
        /// <param name="ruleCode"></param>
        /// <returns></returns>
        public static IList<string> GenNum(string ruleCode, int count)
        {
            IT_CodeGenRepository serviceTemp = new T_CodeGenRepository();
            var expression = ExtLinq.True<T_CodeGenEntity>();
            expression = expression.And(t => t.RuleCode == ruleCode);
            T_CodeGenEntity codeGen = serviceTemp.IQueryable(expression).ToList().FirstOrDefault();
            string format = codeGen.Format;
            DateTime now = DateTime.Now;
            format = format.Replace("[YYYY]", now.Year.ToString().PadLeft(4, '0'));
            format = format.Replace("[YY]", now.Year.ToString().Substring(2, 2));
            format = format.Replace("[MM]", now.Month.ToString().PadLeft(2, '0'));
            format = format.Replace("[DD]", now.Day.ToString().PadLeft(2, '0'));
            format = format.Replace("[H]", codeGen.FixStr.ToString());

            int lastGenData = Convert.ToInt32((codeGen.F_LastModifyTime ?? (Convert.ToDateTime("1990-01-01"))).ToString("yyyyMMdd"));
            int nowGenData = Convert.ToInt32(now.ToString("yyyyMMdd"));

            if (lastGenData == nowGenData)
            {
                codeGen.CurVal = codeGen.CurVal;
            }
            else
            {
                codeGen.CurVal = 0;
            }

            IList<string> numList = new List<string>();
            for (int i = 0; i < count; i++)
            {
                codeGen.CurVal = codeGen.CurVal + 1;
                string numcode = format.Replace("[NUM]", (codeGen.CurVal).ToString().PadLeft(codeGen.Length ?? 0, '0'));
                numList.Add(numcode);
            }
            codeGen.Modify(codeGen.F_Id);

            serviceTemp.Update(codeGen);

            return numList;
        }

        /// <summary>
        /// 获取所有单据或任务流水编码值
        /// </summary>
        /// <param name="ruleCode"></param>
        /// <returns></returns>
        public static string GenNum(string ruleCode, OperatorModel user)
        {
            IT_CodeGenRepository serviceTemp = new T_CodeGenRepository();
            var expression = ExtLinq.True<T_CodeGenEntity>();
            expression = expression.And(t => t.RuleCode == ruleCode);
            T_CodeGenEntity codeGen = serviceTemp.IQueryable(expression).ToList().FirstOrDefault();
            string format = codeGen.Format;
            DateTime now = DateTime.Now;
            format = format.Replace("[YYYY]", now.Year.ToString().PadLeft(4, '0'));
            format = format.Replace("[YY]", now.Year.ToString().PadLeft(2, '0'));
            format = format.Replace("[MM]", now.Month.ToString().PadLeft(2, '0'));
            format = format.Replace("[DD]", now.Day.ToString().PadLeft(2, '0'));
            format = format.Replace("[H]", codeGen.FixStr.ToString());
            format = format.Replace("[NUM]", (codeGen.CurVal + 1).ToString().PadLeft(codeGen.Length ?? 0, '0'));

            codeGen.CurVal = codeGen.CurVal + 1;
            codeGen.ModifyForThread(codeGen.F_Id, user);
            serviceTemp.Update(codeGen);

            return format;
        }

        /// <summary>
        /// 获取所有单据或任务流水编码值
        /// </summary>
        /// <param name="ruleCode"></param>
        /// <returns></returns>
        public static string GenNumForThread(string ruleCode, OperatorModel user)
        {
            IT_CodeGenRepository serviceTemp = new T_CodeGenRepository();
            var expression = ExtLinq.True<T_CodeGenEntity>();
            expression = expression.And(t => t.RuleCode == ruleCode);
            T_CodeGenEntity codeGen = serviceTemp.IQueryable(expression).ToList().FirstOrDefault();
            string format = codeGen.Format;
            DateTime now = DateTime.Now;
            format = format.Replace("[YYYY]", now.Year.ToString().PadLeft(4, '0'));
            format = format.Replace("[YY]", now.Year.ToString().PadLeft(2, '0'));
            format = format.Replace("[MM]", now.Month.ToString().PadLeft(2, '0'));
            format = format.Replace("[DD]", now.Day.ToString().PadLeft(2, '0'));
            format = format.Replace("[H]", codeGen.FixStr.ToString());
            format = format.Replace("[NUM]", (codeGen.CurVal + 1).ToString().PadLeft(codeGen.Length ?? 0, '0'));

            codeGen.CurVal = codeGen.CurVal + 1;
            codeGen.ModifyForThread(codeGen.F_Id, user);
            serviceTemp.Update(codeGen);

            return format;
        }
    }
}
