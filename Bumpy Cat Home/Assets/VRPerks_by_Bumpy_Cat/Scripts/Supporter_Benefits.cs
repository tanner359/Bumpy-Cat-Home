
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.Data;
using Debug = UnityEngine.Debug;

namespace BumpyCat{
public class Supporter_Benefits : UdonSharpBehaviour
{
    //SUPPORT BENEFITS WORK BY: CROSS REFERENCING YOUR VRC+DIS ACCOUNT -> CHECKS FOR TYPE OF SUPPORTER ROLE -> PERKS ACTIVATE.
    [Header("OBJECT REFERENCES")]
    public Supporter_List[] SUPPORT_LIST; //SUPPORTER LISTS, ATTACHED TO INDIVIDUAL TMP TEXT COMPONENTS
    public VRCUrl DISCORD_MEMBERS_URL; //https://discord.com/api/v10/guilds/1235352747923210331/members/search?query=bumpy_cat
    [Header("VRC INFO")]
    public string VRC_NAME;
    [Header("DISCORD INFO")]
    public string DISCORD_NAME;
    public string DISCORD_USER_NAME;
    public string[] DISCORD_ROLES;
    private DataToken DISCORD_MEMBERS = new DataList();

    public bool SERVER_BOOSTER;

    [Header("ROLE ID'S")]
    public long SUPPORTER_ID = 1240128353604730930;
    public long SUPPORTER_PLUS_ID = 1240128353604730930;
    public long SUPPORTER_ULTIMATE_ID = 1240128353604730930;

    public string[] DISCORD_MEMBERS_NAMES = new string[0];

    // Start is called before the first frame update
    public void Start(){
        VRCStringDownloader.LoadUrl(DISCORD_MEMBERS_URL, (IUdonEventReceiver)this);
        Debug.Log($"[{gameObject.name}] Fetching Discord Members...");

        VRC_NAME = Networking.LocalPlayer.displayName;
    }

    public void Init(){
        for(int i = 0; i < SUPPORT_LIST.Length; i++){
            SUPPORT_LIST[i].Initialize(this);
        }
    }
    /*DISCORD JSON STRUCTURE EXAMPLE: 3 DISCORD MEMBERS
    -> STRING LOAD -> JSON DATA
    -> JSON DATA -> DATALIST(3 MEMBERS)
    -> MEMBER -> DATADICTIONARY(12 SETS OF DATA)
    -> SETS W/ EMBED -> DATADICTIONARY(DATA VALUES)
    ->
    -> EX. TO GET NAME: 
    -> JSONDATA(LIST)->USER[i](DICT)->USER["global_name"] 
    */
    public override void OnStringLoadSuccess(IVRCStringDownload data){
        if(VRCJson.TryDeserializeFromJson(data.Result, out DataToken jsonData)){
            if(jsonData.TokenType == TokenType.DataDictionary){
                jsonData.DataDictionary.TryGetValue(1, out DataToken test);
            }
            else if(jsonData.TokenType == TokenType.DataList){ //datalist-> dictionaries -> datalist -> tokens -> data?
                DISCORD_MEMBERS = jsonData;
                for (int i = 0; i < jsonData.DataList.Count;){
                    DataToken a = jsonData.DataList[i];
                    if(a.TokenType == TokenType.DataDictionary){
                        if(a.DataDictionary.TryGetValue("user", TokenType.DataDictionary, out DataToken userData)){
                            DataToken GLOBAL_NAME = userData.DataDictionary["global_name"];
                            if(!GLOBAL_NAME.IsNull){
                                DISCORD_MEMBERS_NAMES = DISCORD_MEMBERS_NAMES.Array_Add_Item(GLOBAL_NAME.String);
                            }
                        }
                    }
                    i++;
                }
            }
            Init();
        }
        else{
                Debug.Log($"Failed to Deserialize json {data.Result} - {jsonData.ToString()}");
        }
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.Log($"[{gameObject.name}] URL Could Not Be Loaded!");
    }

    /// <summary> Parses the rawData and returns a DataList containing all matching key values</summary>
    [RecursiveMethod]
    public DataToken PARSE_JSON(DataToken rawData, string[] keys, int keyindex){
        DataToken result = new DataList();
        if (rawData.TokenType == TokenType.DataDictionary){ // is token a dictionary
            if(rawData.DataDictionary.TryGetValue(keys[keyindex], out DataToken value)){ //try to find a value with key
                if(value.TokenType == TokenType.DataDictionary){
                    DataToken a = PARSE_JSON(value, keys, keyindex+1);
                    return a;
                }
                return value; // return discovered value
            }           
            return new DataToken(TokenType.Null);
        }
        else if(rawData.TokenType == TokenType.DataList){
            for(int i = 0; i < rawData.DataList.Count; i++){
                DataToken parse = PARSE_JSON(rawData.DataList[i], keys, keyindex);
                result.DataList.Add(parse);
            }
            return result; 
        }
        Debug.Log("Supporter_Benefits.PARSE_JSON() Returned : [Code 0]");
        return result;
    }
    public string Get_Supporters(){

        string result = Get_Supporters(SUPPORTER_ID) + Get_Supporters(SUPPORTER_PLUS_ID) + Get_Supporters(SUPPORTER_ULTIMATE_ID);
        return result;
    }
    public string Get_Supporters(long role_id){
        string result = "";

        string[] names_keys = {"user", "global_name"};
        DataList names = PARSE_JSON(DISCORD_MEMBERS, names_keys, 0).DataList;
        
        string[] roles_keys = {"roles"}; 
        DataList roles = PARSE_JSON(DISCORD_MEMBERS, roles_keys , 0).DataList;

        for(int i = 0; i < DISCORD_MEMBERS.DataList.Count; i++){
            for(int j = 0; j < roles[i].DataList.Count; j++){
                if(roles[i].DataList[j].String == role_id.ToString()){
                    result += names[i].String + "\n";
                }
            }
        }
        return result;
    }
    /// <summary>Get names of Discord members who have the supporter role</summary>
    public string Get_Supporter_Names(){
        string[] keys = {"user", "global_name"};
        DataList result = PARSE_JSON(DISCORD_MEMBERS, keys, 0).DataList;
        string names = "";
        for (int i = 0; i < result.Count; i++)
        {
            if(result[i].TokenType != TokenType.Null){
                names += result[i].String + " ";
            }
        }
        return names;
    }
}
}