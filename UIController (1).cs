using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class UIController : MonoBehaviour {

    public GameObject itemUIPrefab;
    public GameObject inventoryScreenGO;
    public GameObject buttonsContainerGO;
    public Transform inventoryContainer;

    XmlDocument itemDataXml;  //https://docs.microsoft.com/es-es/dotnet/api/system.xml.xmldocument?view=net-5.0

    private void Awake()
    {
        TextAsset xmlTextAsset = Resources.Load<TextAsset>("XML/InventoryItemData");
        itemDataXml = new XmlDocument();
        itemDataXml.LoadXml(xmlTextAsset.text);
        inventoryScreenGO.SetActive(false);
    }

    public void FindAllItems()
    {
        Debug.Log("Finding all items");

        XmlNodeList items = itemDataXml.SelectNodes("/InventoryItems/InventoryItem");

        foreach (XmlNode item in items)
        {
            SpawnInventoryItem(item);
        }

        ShowInventoryItems();
    }

    public void FindItemsOfType(string itemType)
    {
        Debug.Log("Finding all items of type: " + itemType);

        XmlNodeList items = itemDataXml.SelectNodes("/InventoryItems/InventoryItem[@Type='" + itemType + "']");

        foreach (XmlNode item in items)
        {
            SpawnInventoryItem(item);
        }

        ShowInventoryItems();
    }

    public void FindItemsWithID(string itemID)
    {
        XmlNode curNode = itemDataXml.SelectSingleNode("/InventoryItems/InventoryItem[@ID='" + itemID + "']");
        if (curNode == null)
        {
            Debug.LogError("Error could not find Inventory Item with ID: " + itemID + " in IeventoryItemData.xml");
            return;
        }

        SpawnInventoryItem(curNode);
        ShowInventoryItems();
    }

    void ShowInventoryItems()
    {
        buttonsContainerGO.SetActive(false);
        inventoryScreenGO.SetActive(true);
    }

    void SpawnInventoryItem(XmlNode item)
    {
        Debug.Log("Spawning Inventory Item");

        GameObject newItemUI = GameObject.Instantiate(itemUIPrefab, inventoryContainer);

        InventoryItem newInventoryItem = new InventoryItem(item);
        newInventoryItem.UpdateInventoryUI(newItemUI);
    }

    public void OnButtonBack()
    {
        foreach(Transform t in inventoryContainer)
        {
            Destroy(t.gameObject);
        }

        inventoryScreenGO.SetActive(false);
        buttonsContainerGO.SetActive(true);
    }

    class InventoryItem
    {
        public string itemID { get; private set; }
        public string itemType { get; private set; }
        public string itemTitle { get; private set; }
        public string itemDescription { get; private set; }
        public Color bgColor { get; private set; }
        public Texture itemImage { get; private set; }

        public InventoryItem(XmlNode curItemNode)
        {
            itemID = curItemNode.Attributes["ID"].Value;
            itemType = curItemNode.Attributes["Type"].Value;
            itemTitle = curItemNode["TituloItem"].InnerText;
            itemDescription = curItemNode["ItemDescripcion"].InnerText;

            XmlNode colorNode = curItemNode.SelectSingleNode("Color");

            float bgR = float.Parse(colorNode["r"].InnerText);
            float bgG = float.Parse(colorNode["g"].InnerText);
            float bgB = float.Parse(colorNode["b"].InnerText);
            float bgA = float.Parse(colorNode["a"].InnerText);

            bgR = NormalizeColorValue(bgR);
            bgG = NormalizeColorValue(bgG);
            bgB = NormalizeColorValue(bgB);
            bgA = NormalizeColorValue(bgA);

            Debug.Log("Color of " + itemTitle + " is " + bgR + ", " + bgG + ", " + bgB + ", " + bgA);

            bgColor = new Color(bgR, bgG, bgB, bgA);


            string pathToImage = "InventoryIcons/" + curItemNode["Image"].InnerText;

            itemImage = Resources.Load<Texture2D>(pathToImage);
        }

        float NormalizeColorValue(float value)
        {
            value = value / 255f;
            return value;
        }

        public void UpdateInventoryUI(GameObject inventoryUI)
        {
            Transform inventoryUITransform = inventoryUI.transform;

            Image itemBGPanel;
            RawImage itemRawImage;
            Text itemTitleText;
            Text itemDescriptionText;

            itemBGPanel = inventoryUITransform.GetComponent<Image>();
            Transform itemBGPanelTransform = itemBGPanel.GetComponent<Transform>();
            itemRawImage = itemBGPanelTransform.Find("ItemRawImage").GetComponent<RawImage>();
            itemTitleText = itemBGPanelTransform.Find("ItemTitleText").GetComponent<Text>();
            itemDescriptionText = itemBGPanelTransform.Find("ItemDescriptionText").GetComponent<Text>();

            if (itemBGPanel == null)
            {
                Debug.LogError("Error could not find itemBGPanel on inventoryUITransform: " + inventoryUITransform.gameObject.name);
            }
            if (itemRawImage == null)
            {
                Debug.LogError("Error could not find itemRawImage on inventoryUITransform: " + inventoryUITransform.gameObject.name);
            }
            if (itemTitleText == null)
            {
                Debug.LogError("Error could not find itemTitleText on inventoryUITransform: " + inventoryUITransform.gameObject.name);
            }
            if (itemDescriptionText == null)
            {
                Debug.LogError("Error could not find itemDescriptionText on inventoryUITransform: " + inventoryUITransform.gameObject.name);
            }

            itemBGPanel.color = bgColor;
            itemRawImage.texture = itemImage;
            itemTitleText.text = itemTitle;
            itemDescriptionText.text = itemDescription;
        }
    }
}
