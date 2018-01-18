using System.Collections.Generic;
using System.Text;

namespace SetCover.Objects
{
    /// <summary>
    /// This class assigns a Unique ID value to each string value sent to GetGlobalID
    /// </summary>
    /// <remarks>It is used to keep the string values assigned to nodeName from getting too long by storing a list of numbers instead of a list of peptides or proteins</remarks>
    public class GlobalIDContainer
    {
        protected Dictionary<string, int> mItemsByName;
        protected Dictionary<int, string> mItemsByID;

        // Constructor
        public GlobalIDContainer()
        {
            mItemsByName = new Dictionary<string, int>();
            mItemsByID = new Dictionary<int, string>();
        }

        public int GetGlobalID(string itemName)
        {
            if (!mItemsByName.TryGetValue(itemName, out var itemID))
            {
                itemID = mItemsByName.Count;
                mItemsByName.Add(itemName, itemID);
                mItemsByID.Add(itemID, itemName);
            }
            return itemID;
        }

        public string GetNameByID(int itemID)
        {
            if (!mItemsByID.TryGetValue(itemID, out var itemName))
            {
                return string.Empty;
            }
            return itemName;
        }

        public string IDListToNameListString(string idList, char sepChar)
        {
            var nodeNames = IDListToNameList(idList, sepChar);
            var nodeNameList = new StringBuilder();

            foreach (var item in nodeNames)
            {
                if (nodeNameList.Length > 0)
                    nodeNameList.Append(sepChar);

                nodeNameList.Append(item);
            }

            return nodeNameList.ToString();
        }

        public List<string> IDListToNameList(string idList, char sepChar)
        {
            var itemIDs = idList.Split(sepChar);
            var nodeNames = new List<string>();

            foreach (var itemIDText in itemIDs)
            {
                if (int.TryParse(itemIDText, out var itemID))
                {
                    var itemName = GetNameByID(itemID);
                    nodeNames.Add(itemName);
                }
            }

            return nodeNames;
        }
    }
}
