/*:
 * AITSYS_RPC.js
 * @plugindesc This plugin provides support for discord rpc.
 * 
 * @param rpcip
 * @text RPC Mode
 * @type boolean
 * @desc RPC Mode (testing|production)
 * @on Production
 * @off Testing
 * @default false
 * 
 * @author Lala Sabathil
 *
 * @help
 * ============================================================================
 * Introductions
 * ============================================================================
 * This plugin provides support for discord rpc.
 * ============================================================================
 * Terms of Use
 * ============================================================================
 * This plugin is free for use with credits in non-commercial and commercial
 * projects as well. Editing this plugin is forbidden!
 *
 * Contact: Lala Sabathil <aiko@aiko-it-systems.eu> on Discord Aiko~Chan#0001
 * ============================================================================
 * Change Log
 * ============================================================================
 * 2021.30.01 (v1.0.0) :
 * * Create first version
 * 2021.31.01 (v1.1.0) :
 * + Added the map-changed getName Event
 * 2021.18.04 (v1.3.0) :
 * * On null ignore
 */

var Imported = Imported || {};
Imported.AITSYS_RPC = true;

(function () {
  function parseParameters(params) {
    var obj;
    try {
      obj = JsonEx.parse(params && typeof params === 'object' ? JsonEx.stringify(params) : params);
    } catch (e) {
          return params;
    }
    if (obj && typeof obj === 'object') {
      Object.keys(obj).forEach(function (key) {
        obj[key] = parseParameters(obj[key]); 
        if (obj[key] === '') {
          obj[key] = null;
        }
      });
    }
    return obj;
  }
  var params = parseParameters(PluginManager.parameters('AITSYS_RPC'));

  var ipaddr = params['rpcip'];

  var gmset = Game_Map.prototype.setup;
  Game_Map.prototype.setup = function(mid) {
    gmset.call(this, mid);
    if($dataMapInfos[$gameMap._mapId].name != null) {
      var name = $dataMapInfos[$gameMap._mapId].name
      if(ipaddr) {
        var ip = '127.0.0.1';
      } else {
        var ip = '88.99.239.173';
      }
      console.log("Transmitting map destination change to |" + ip + "|:" + name);
      setRPC(name, ip);
    }
  };

  var net = require('net');
  function setRPC(name, ip) {
    var client = new net.Socket();
    client.setEncoding("utf8");
    client.connect(59090, ip, function() {
      client.write("logo " + name + " \n");
    });
    
    client.on('data', function(data) {
      console.log(data);
      client.destroy();
    });
    
    client.on('close', function() {
      console.log("Map destination change handled");
    });
  }
})();
