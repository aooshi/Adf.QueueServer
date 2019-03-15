
function CreateQueue(frm) {
    var queue = frm.queue.value;
    var url = "/queue/createqueue.json?queue=" + queue + "&requestid=" + Math.random();

    ajax({ 
        url: url,
        success: function (result, xhr, opts) {
/*
{
  "action":"delete/lcancel/rcancel/createqueue/deletequeue",
  "id":"88bea088837d4743af67a2d49e6d08d1",
  "queue": "/order/new",
  "result":"ok or failure message"
}
*/
            if (result.action == "createqueue")
            {
                if (result.result == "ok")
                {
                   WorkSuccess(' queue create success.');
                }
                else
                {
                    WorkFailure(' ' + result.result);
                }
            }

        },
        error: function (result, xhr, opts) {
             WorkFailure(' ' + result);
        }
    });
        
    WorkProcess();
}

function DeleteQueue(frm) {
    var queue = frm.queue.value;
    var url = "/queue/deletequeue.json?queue=" + queue + "&requestid=" + Math.random();

    if (confirm("delete queue " + queue + " \nare you sure?") == false) {
        return false;
    }

    ajax({ 
        url: url,
        success: function (result, xhr, opts) {
/*
{
  "action":"delete/lcancel/rcancel/createqueue/deletequeue",
  "id":"88bea088837d4743af67a2d49e6d08d1",
  "queue": "/order/new",
  "result":"ok or failure message"
}
*/
            if (result.action == "deletequeue")
            {
                if (result.result == "ok")
                {
                   WorkSuccess(' queue delete success.');
                }
                else
                {
                    WorkFailure(' ' + result.result);
                }
            }

        },
        error: function (result, xhr, opts) {
            WorkFailure(' ' + result);
        }
    });
        
    WorkProcess();
}


function ClearQueue(frm) {
    var queue = frm.queue.value;
    var url = "/queue/clear.json?queue=" + queue + "&requestid=" + Math.random();

    if (confirm("clear queue " + queue + " \nare you sure?") == false) {
        return false;
    }

    ajax({
        url: url,
        success: function (result, xhr, opts) {
            /*
            {
            "action":"delete/lcancel/rcancel/createqueue/deletequeue",
            "id":"88bea088837d4743af67a2d49e6d08d1",
            "queue": "/order/new",
            "result":"ok or failure message"
            }
            */
            if (result.action == "clear") {
                if (result.result == "ok") {
                    WorkSuccess(' clear success.');
                }
                else {
                    WorkFailure(' ' + result.result);
                }
            }

        },
        error: function (result, xhr, opts) {
            WorkFailure(' ' + result);
        }
    });

    WorkProcess();
}

function WorkProcess(message)
{
    var msg = document.getElementById('msg');
    msg.style = 'color:#ff8000';
    msg.innerHTML = "process ...";
}

function WorkSuccess(message)
{
    var msg = document.getElementById('msg');
    msg.style = 'color:green';
    msg.innerHTML = message;
}

function WorkFailure(message)
{
    var msg = document.getElementById('msg');
    msg.style = 'color:red';
    msg.innerHTML = message;
}
