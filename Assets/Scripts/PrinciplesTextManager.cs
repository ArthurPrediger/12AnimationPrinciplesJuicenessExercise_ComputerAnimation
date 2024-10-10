using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PrinciplesTextManager : MonoBehaviour
{
    private TMPro.TextMeshProUGUI principlesText;
    private HashSet<PrincipleInfo> textContent = new();
    private List<PrincipleInfo> textContentOrdered = new();

    // Start is called before the first frame update
    void Start()
    {
        principlesText = FindObjectOfType<TextMeshProUGUI>();
        principlesText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private class PrincipleInfo
    {
        public string name;
        public Color color;
        public Coroutine removePrincipleCoro;

        public PrincipleInfo(string name, Color color)
        {
            this.name = name;
            this.color = color;
        }

        public static bool operator ==(PrincipleInfo pInfo1, PrincipleInfo pInfo2)
        {
            return pInfo1.name == pInfo2.name && pInfo1.color == pInfo2.color;
        }

        public static bool operator !=(PrincipleInfo pInfo1, PrincipleInfo pInfo2)
        {
            return pInfo1.name != pInfo2.name || pInfo1.color != pInfo2.color;
        }

        public override bool Equals(object other)
        {
            return this == (PrincipleInfo)other;
        }

        public override int GetHashCode()
        {
            return (name, color).GetHashCode();
        }
    }

    public void AddPrinciple(string principleName, Color objColor)
    {
        PrincipleInfo pInfo = new PrincipleInfo(principleName, objColor);

        if(textContent.TryGetValue(pInfo, out PrincipleInfo actualInfo))
        {
            StopCoroutine(actualInfo.removePrincipleCoro);
            textContent.Remove(actualInfo);
            textContentOrdered.Remove(actualInfo);
        }

        textContent.Add(pInfo);
        textContentOrdered.Add(pInfo);
        textContentOrdered.Last().removePrincipleCoro = StartCoroutine(RemovePrinciple(pInfo));
        UpdateTextContent();
    }

    private IEnumerator RemovePrinciple(PrincipleInfo principleInfo)
    {
        yield return new WaitForSeconds(5.0f);

        if (textContent.Remove(principleInfo))
        {
            textContentOrdered.RemoveAt(0);
            UpdateTextContent();
        }
    }

    private void UpdateTextContent()
    {
        principlesText.text = "";

        foreach (var principleInfo in textContentOrdered)
        {
            principlesText.text += "<color=#" + ColorUtility.ToHtmlStringRGBA(principleInfo.color) + ">" + principleInfo.name + "</color>\n";
        }
    }
}
