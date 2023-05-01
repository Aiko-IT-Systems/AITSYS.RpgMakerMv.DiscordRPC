var Imported=Imported||{};Imported.AITSYS_RPC=!0;
(function(){function f(d){console.info("Rpc communication started.");console.info("Sending: "+d);var c=new l.Socket;c.setEncoding("utf8");c.connect(a.Param.RpcPort,"127.0.0.1",function(){c.write(d)});c.on("data",function(b){var e=JSON.parse(b);0!=e.type?console.error("Error: "+e.message):console.info(b);c.destroy()});c.on("close",function(){console.info("Rpc communication finished")})}function m(){const d=process.cwd();let c=[];null!=a.Param.SteamAppId&&""!=a.Param.SteamAppId&&(c.push("--sapp"),c.push(a.Param.SteamAppId));
59090!=a.Param.RpcPort&&(c.push("--port"),c.push(a.Param.RpcPort));c.push("--dapp");c.push(a.Param.DiscordAppId);h=n.execFile(p.join(d,"/"+a.Param.RpcExecutableFolder,"/AITSYS.RpgMakerMv.Rpc.exe"),c,(b,e,q)=>{b&&(console.error(b),alert(b))})}function g(d=!0){d&&m();try{let b=JSON.stringify({rpc_cmd:1,rpc_data:{large_asset_key:a.Param.LargeAssetKey,large_asset_text:a.Param.LargeAssetText,small_asset_key:a.Param.SmallAssetKey,small_asset_text:a.Param.SmallAssetText,details:a.Param.Details,state:a.Param.State}}),
e=JSON.stringify({rpc_cmd:0,rpc_config:{use_button_one:a.Param.UseButtonOne,use_button_two:a.Param.UseButtonTwo,button_one_config:{label:a.Param.LabelOne,url:a.Param.UrlOne},button_two_config:{label:a.Param.LabelTwo,url:a.Param.UrlTwo}}});f(e);f(b)}catch(b){console.trace("Rpc communication failed")}var c=Game_Map.prototype.setup;Game_Map.prototype.setup=function(b){c.call(this,b);b=$dataMapInfos[$gameMap._mapId];if(null!=b.name&&0!=b.params.length&&"rpc"==b.params[0].type){b=b.params[0].data;b=JSON.stringify({rpc_cmd:1,
rpc_data:{large_asset_key:b.large_asset_key??a.Param.LargeAssetKey,large_asset_text:b.large_asset_text??a.Param.LargeAssetText,small_asset_key:b.small_asset_key??a.Param.SmallAssetKey,small_asset_text:b.small_asset_text??a.Param.SmallAssetText,details:b.details,state:b.state}});try{f(b)}catch(e){console.trace("Rpc communication failed")}}}}var a=a||{};a.Version="4.0.0";a.Parameters=PluginManager.parameters("AITSYS_RPC");a.Param=a.Param||{};a.Param.DiscordAppId=String(a.Parameters["Discord App Id"]);
a.Param.SteamAppId=String(a.Parameters["Steam App Id"]);a.Param.RpcExecutableFolder=String(a.Parameters["RPC Executable Folder"]);a.Param.RpcPort=Number(a.Parameters["RPC Port"]);a.Param.UseButtonOne=!!a.Parameters["Use Button One"];a.Param.UseButtonTwo=!!a.Parameters["Use Button Two"];a.Param.LabelOne=String(a.Parameters["Label One"]);a.Param.UrlOne=String(a.Parameters["Url One"]);a.Param.LabelTwo=String(a.Parameters["Label Two"]);a.Param.UrlTwo=String(a.Parameters["Url Two"]);a.Param.LargeAssetKey=
String(a.Parameters["Large Asset Key"]);a.Param.LargeAssetText=String(a.Parameters["Large Asset Text"]);a.Param.SmallAssetKey=String(a.Parameters["Small Asset Key"]);a.Param.SmallAssetText=String(a.Parameters["Small Asset Text"]);a.Param.Details=String(a.Parameters.Details);a.Param.State=String(a.Parameters.State);var k=require("nw.gui").App.argv;let p=require("path"),l=require("net"),n=require("child_process"),h=null;k=k.indexOf("enable-rpc");chrome.commandLinePrivate.hasSwitch("enable-rpc",function(d){d?
(console.info("RPC enabled (chrome param found)"),g(!0)):console.info("RPC disabled (chrome param not found)")});-1<k?(console.info("RPC enabled (param found)"),g(!0)):console.info("RPC disabled (param not found)");g(!1);window.callRpc=function(d){f(d)};window.stopRpc=function(){null!=h&&h.kill()};window.enableRpc=function(d=!1){g(d)}})();
//# sourceMappingURL=AITSYS_RPC.js.map
