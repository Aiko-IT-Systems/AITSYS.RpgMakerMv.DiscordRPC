/*:
 * AITSYS_RPC.js
 * @plugindesc This plugin provides support for discord rpc.
 *
 * @author Lala Sabathil
 *
 * @version 4.0.0
 * @filename AITSYS_RPC.js
 * @url https://github.com/Aiko-IT-Systems/AITSYS.RpgMakerMv.DiscordRPC/blob/main/src/AITSYS_RPC.js
 *
 * @param --- Default Config ---
 * @default
 *
 * @param Discord App Id
 * @parent --- Default Config ---
 * @type string
 * @desc The discord application id bound to the rich presence
 * @default 805569792446562334
 *
 * @param Steam App Id
 * @parent --- Default Config ---
 * @type string
 * @desc The steam app id bound to the rich presence
 * @default
 *
 * @param RPC Executable Folder
 * @parent --- Default Config ---
 * @type string
 * @desc The folder for the rpc executable, relative to the game folder
 * @default RPC Plugin
 *
 * @param RPC Port
 * @parent --- Default Config ---
 * @type number
 * @desc The port for the rpc
 * @default 59090
 *
 * @param Use Button One
 * @parent --- Default Config ---
 * @type boolean
 * @on YES
 * @off NO
 * @desc Do you want to display button one on the rpc?
 * NO - false     YES - true
 * @default true
 *
 * @param Use Button Two
 * @parent --- Default Config ---
 * @type boolean
 * @on YES
 * @off NO
 * @desc Do you want to display button two on the rpc?
 * NO - false     YES - true
 * @default true
 *
 * @param --- Button One ---
 * @default
 *
 * @param Label One
 * @parent --- Button One ---
 * @type string
 * @desc The button label
 * @default Get RPC Extension
 *
 * @param Url One
 * @parent --- Button One ---
 * @type string
 * @desc The button url
 * @default https://github.com/Aiko-IT-Systems/AITSYS.RpgMakerMv.Rpc
 *
 * @param --- Button Two ---
 * @default
 *
 * @param Label Two
 * @parent --- Button Two ---
 * @type string
 * @desc The button label
 * @default RPC Extension Support
 *
 * @param Url Two
 * @parent --- Button Two ---
 * @type string
 * @desc The button url
 * @default https://discord.gg/Uk7sggRBTm
 *
 * @param --- Default Data ---
 * @default
 *
 * @param Large Asset Key
 * @parent --- Default Data ---
 * @type string
 * @desc Key for the uploaded image for the large asset or url
 * @default logo
 *
 * @param Large Asset Text
 * @parent --- Default Data ---
 * @type string
 * @desc Tooltip for the large asset
 * @default RMMV Game
 *
 * @param Small Asset Key
 * @parent --- Default Data ---
 * @type string
 * @desc Key for the uploaded image for the small asset or url
 * @default
 *
 * @param Small Asset Text
 * @parent --- Default Data ---
 * @type string
 * @desc Tooltip for the small asset
 * @default
 *
 * @param Details
 * @parent --- Default Data ---
 * @type string
 * @desc What the player is currently doing
 * @default Playing Game
 *
 * @param State
 * @parent --- Default Data ---
 * @type string
 * @desc The users current party status
 * @default
 *
 * @help
 *
 * ============================================================================
 * Introductions
 * ============================================================================
 *
 * This plugin provides support for discord rpc.
 *
 * ============================================================================
 * Terms of Use
 * ============================================================================
 *
 * This plugin is free for use with credits in non-commercial and commercial
 * projects as well. Editing this plugin is forbidden!
 *
 * Contact: Lala Sabathil (aitsys.dev) on Discord Catra#9999
 *
 * ============================================================================
 * Change Log
 * ============================================================================
 *
 * 2021.30.01 (v1.0.0):
 * * Create first version
 * 2021.31.01 (v1.1.0):
 * + Added the map-changed getName Event
 * 2021.18.04 (v1.3.0):
 * * On null ignore
 * 2022.12.10 (v2.0.0):
 * + Added ignore if rpc isn't running
 * 2023.14.01 (v3.0.0):
 * * Reworked the plugin to control steam dlc
 * 2023.XX.XX (v4.0.0):
 * * Rewrote plugin for general use
 */
var Imported=Imported||{};Imported.AITSYS_RPC=!0;
(function(){function f(d){console.info("Rpc communication started.");console.info("Sending: "+d);var c=new l.Socket;c.setEncoding("utf8");c.connect(a.Param.RpcPort,"127.0.0.1",function(){c.write(d)});c.on("data",function(b){var e=JSON.parse(b);0!=e.type?console.error("Error: "+e.message):console.info(b);c.destroy()});c.on("close",function(){console.info("Rpc communication finished")})}function m(){const d=process.cwd();let c=[];null!=a.Param.SteamAppId&&""!=a.Param.SteamAppId&&(c.push("--sapp"),c.push(a.Param.SteamAppId));
59090!=a.Param.RpcPort&&(c.push("--port"),c.push(a.Param.RpcPort));c.push("--dapp");c.push(a.Param.DiscordAppId);g=n.execFile(p.join(d,"/"+a.Param.RpcExecutableFolder,"/AITSYS.RpgMakerMv.Rpc.exe"),c,(b,e,q)=>{b&&(console.error(b),alert(b))})}function h(d=!0){d&&m();try{let b=JSON.stringify({rpc_cmd:1,rpc_data:{large_asset_key:a.Param.LargeAssetKey,large_asset_text:a.Param.LargeAssetText,small_asset_key:a.Param.SmallAssetKey,small_asset_text:a.Param.SmallAssetText,details:a.Param.Details,state:a.Param.State}}),
e=JSON.stringify({rpc_cmd:0,rpc_config:{use_button_one:a.Param.UseButtonOne,use_button_two:a.Param.UseButtonTwo,button_one_config:{label:a.Param.LabelOne,url:a.Param.UrlOne},button_two_config:{label:a.Param.LabelTwo,url:a.Param.UrlTwo}}});f(e);f(b)}catch(b){console.trace("Rpc communication failed")}var c=Game_Map.prototype.setup;Game_Map.prototype.setup=function(b){c.call(this,b);b=$dataMapInfos[$gameMap._mapId];if(null!=b.name&&0!=b.params.length&&"rpc"==b.params[0].type){b=b.params[0].data;b=JSON.stringify({rpc_cmd:1,
rpc_data:{large_asset_key:b.large_asset_key??a.Param.LargeAssetKey,large_asset_text:b.large_asset_text??a.Param.LargeAssetText,small_asset_key:b.small_asset_key??a.Param.SmallAssetKey,small_asset_text:b.small_asset_text??a.Param.SmallAssetText,details:b.details,state:b.state}});try{f(b)}catch(e){console.trace("Rpc communication failed")}}}}var a=a||{};a.Version="4.0.0";a.Parameters=PluginManager.parameters("AITSYS_RPC");a.Param=a.Param||{};a.Param.DiscordAppId=String(a.Parameters["Discord App Id"]);
a.Param.SteamAppId=String(a.Parameters["Steam App Id"]);a.Param.RpcExecutableFolder=String(a.Parameters["RPC Executable Folder"]);a.Param.RpcPort=Number(a.Parameters["RPC Port"]);a.Param.UseButtonOne=!!a.Parameters["Use Button One"];a.Param.UseButtonTwo=!!a.Parameters["Use Button Two"];a.Param.LabelOne=String(a.Parameters["Label One"]);a.Param.UrlOne=String(a.Parameters["Url One"]);a.Param.LabelTwo=String(a.Parameters["Label Two"]);a.Param.UrlTwo=String(a.Parameters["Url Two"]);a.Param.LargeAssetKey=
String(a.Parameters["Large Asset Key"]);a.Param.LargeAssetText=String(a.Parameters["Large Asset Text"]);a.Param.SmallAssetKey=String(a.Parameters["Small Asset Key"]);a.Param.SmallAssetText=String(a.Parameters["Small Asset Text"]);a.Param.Details=String(a.Parameters.Details);a.Param.State=String(a.Parameters.State);var k=require("nw.gui").App.argv;let p=require("path"),l=require("net"),n=require("child_process"),g=null;k=k.indexOf("enable-rpc");chrome.commandLinePrivate.hasSwitch("enable-rpc",function(d){d?
(console.info("RPC enabled (chrome param found)"),h(!0)):console.info("RPC disabled (chrome param not found)")});-1<k?(console.info("RPC enabled (param found)"),h(!0)):console.info("RPC disabled (param not found)");window.callRpc=function(d){f(d)};window.stopRpc=function(){null!=g&&g.kill()};window.enableRpc=function(d=!1){h(d)}})();
//# sourceMappingURL=AITSYS_RPC.js.map
