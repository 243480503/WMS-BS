/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System.Collections.Generic;
using System.Text;

namespace MST.Code
{
    public static class TreeGrid
    {
        public static string TreeGridJson(this List<TreeGridModel> data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ \"rows\": [");
            sb.Append(TreeGridJson(data, -1, "0"));
            sb.Append("]}");
            return sb.ToString();
        }
        private static string TreeGridJson(List<TreeGridModel> data, int index, string parentId)
        {
            StringBuilder sb = new StringBuilder();
            var ChildNodeList = data.FindAll(t => t.parentId == parentId);
            if (ChildNodeList.Count > 0) { index++; }
            foreach (TreeGridModel entity in ChildNodeList)
            {
                string strJson = entity.entityJson;
                strJson = strJson.Insert(1, "\"loaded\":" + (entity.loaded == true ? false : true).ToString().ToLower() + ",");
                strJson = strJson.Insert(1, "\"expanded\":" + (entity.expanded).ToString().ToLower() + ",");
                strJson = strJson.Insert(1, "\"isLeaf\":" + (entity.isLeaf == true ? false : true).ToString().ToLower() + ",");
                strJson = strJson.Insert(1, "\"parent\":\"" + parentId + "\",");
                strJson = strJson.Insert(1, "\"level\":" + index + ",");
                sb.Append(strJson);
                sb.Append(TreeGridJson(data, index, entity.id));
            }
            return sb.ToString().Replace("}{", "},{");
        }
    }
}
