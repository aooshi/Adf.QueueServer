//GET:	ajax("url",function(result,xhr,opts){});
//POST:	ajax("url",{username:""},function(result,xhr,opts){});
//ajax({url:"",data:{},success:function(result,xhr,opts){},error:function(result,xhr,opts){},complete:function(result,xhr,opts){},json:true,start:function(xhr,opts){} });
//window.ajax_default_error = function(result,xhr,opts){}
//window.ajax_default_start = function(xhr,opts){}
//window.ajax_default_complete = function(result,xhr,opts){}
function ajax() {
	var xhr,opts = {
		error:window.ajax_default_error
		,start:window.ajax_default_start
		,complete:window.ajax_default_complete
	},args=arguments;
	function create() {
		if (window.XMLHttpRequest) {
			return new XMLHttpRequest();
		} else if (window.ActiveXObject) {
			var msxmls = ["MSXML4", "MSXML3", "MSXML2", "MSXML", "Microsoft"];
			for (var i = 0,l=msxmls.length; i < l; i++) {
				try {
					return new ActiveXObject(msxmls[i] + ".XMLHTTP");
				} catch (e) {}
			}
		}
	}
	function build_data() {
		if (opts.data)
		{
			var val = '';
			for (var n in opts.data) {
				val += "&" + n + "=" + encodeURIComponent(opts.data[n]);
			}
			return val == '' ? '' : val.substr(1);
		}
		else 
			return '';
	}
	function build_json(s)
	{
		if (s == "") return null;
		if (JSON.parse) return JSON.parse(s);
		return eval(s);
	}
	
	
	//ajax('url',{},function(result){});
	if (args.length == 3)
	{
		opts.url = args[0];
		opts.data = args[1];
		opts.success = args[2];
	}
	
	//ajax('url',function(result){});
	if (args.length == 2)
	{
		opts.url = args[0];
		opts.success = args[1];
	}
	
	//ajax({url:'',data:{},success:function(result){},error:function(xhr,url,data){},complete:function(xhr,url,data){} },json:true,start:function(xhr,url,data){});
	if (args.length == 1)
	{
		for(var n in args[0])
			opts[n] = args[0][n];
	}
	//
	opts.mode = (opts.data) ? "POST" : "GET";
		
	if (opts.url == undefined || opts.url == '' || opts.url == null)
		throw new Error("no set ajax url");
	
	if (opts.success == undefined || typeof opts.success != 'function')
		throw new Error("no set ajax success");
		
	if (typeof(xhr = create()) == undefined)
		throw new Error("ajax error : no support XMLHttpRequest");
	
	xhr.onreadystatechange = function() {
		if (xhr.readyState == 4) 
		{
			var rd;
			//
			if (opts.json && xhr.status == 200)
				rd = build_json(xhr.responseText);
			else
			{
				var response_type = (xhr.getResponseHeader("Content-Type") || "");
				//remove charset: text/html;charset=utf-8
				response_type = response_type.toLowerCase().split(';')[0].replace(/(^\s*)|(\s*$)/g, "");
				switch(response_type)
				{
					case "text/xml":
						rd = xhr.responseXML;
						break;
					case "text/json":
					case "application/json":
					case "application/x-json":
						rd = build_json(xhr.responseText);
						break;
					case "text/javascript":
					case "application/javascript":
					case "application/x-javascript":
						rd = eval(xhr.responseText);
						break;
					default:
						rd = xhr.responseText;
						break;
				}
			}
			if (xhr.status == 200) 
				opts.success(rd,xhr,opts);
			else if (opts.error)
				opts.error(rd,xhr,opts);
			if (opts.complete)
				opts.complete(rd,xhr,opts);
		}
	};
	xhr.open(opts.mode, opts.url, true);
	xhr.setRequestHeader("X-Requested-With", 'XMLHttpRequest');
	if (opts.json)
		xhr.setRequestHeader("Accept", 'application/json');
	var send_data = null;
	if (opts.mode == "POST")
	{
		xhr.setRequestHeader("Content-Type","application/x-www-form-urlencoded");
		send_data = build_data();
	}
	if (opts.start)
		opts.start(xhr,opts);
	xhr.send(send_data);
}