// export SLACK_WEBHOOK=https://hooks.slack.com/services/???????????????????????????????????????
// export SLACK_TITLE=example title
// export SLACK_MESSAGE=example message

const https = require('https');

if (!process.env.SLACK_WEBHOOK ) {
  throw Error("missing slack webhook, use SLACK_WEBHOOK")
}

if (!process.env.SLACK_TITLE ) {
  throw Error("missing title, use SLACK_TITLE")
}

const yourWebHookURL = process.env.SLACK_WEBHOOK; // PUT YOUR WEBHOOK URL HERE
let userAccountNotification = {
  'username': 'Notification', // This will appear as user name who posts the message
  'text': process.env.SLACK_TITLE,
  'icon_emoji': ':bangbang:'
};

if (process.env.SLACK_MESSAGE) {
  userAccountNotification.attachments = [{ // this defines the attachment block, allows for better layout usage
    "color": "#eed140", // color of the attachments sidebar.
    "fields": [ // actual fields
      {
        "value": process.env.SLACK_MESSAGE, // Custom field
        "short": false // long fields will be full width
      }
    ]
  }]
}

/**
 * Handles the actual sending request.
 * We're turning the https.request into a promise here for convenience
 * @param webhookURL
 * @param messageBody
 * @return {Promise}
 */
function sendSlackMessage (webhookURL, messageBody) {
  // make sure the incoming message body can be parsed into valid JSON
  try {
    messageBody = JSON.stringify(messageBody);
  } catch (e) {
    throw new Error('Failed to stringify messageBody', e);
  }

  // Promisify the https.request
  return new Promise((resolve, reject) => {
    // general request options, we defined that it's a POST request and content is JSON
    const requestOptions = {
      method: 'POST',
      header: {
        'Content-Type': 'application/json'
      }
    };

    // actual request
    const req = https.request(webhookURL, requestOptions, (res) => {
      let response = '';


      res.on('data', (d) => {
        response += d;
      });

      // response finished, resolve the promise with data
      res.on('end', () => {
        resolve(response);
      })
    });

    // there was an error, reject the promise
    req.on('error', (e) => {
      reject(e);
    });

    // send our message body (was parsed to JSON beforehand)
    req.write(messageBody);
    req.end();
  });
}

// main
(async function () {
  if (!yourWebHookURL) {
    console.error('Please fill in your Webhook URL');
  }

  console.log('Sending slack message');
  try {
    const slackResponse = await sendSlackMessage(yourWebHookURL, userAccountNotification);
    console.log('Message response', slackResponse);
  } catch (e) {
    console.error('There was a error with the request', e);
  }
})();

// source: https://blog.nodeswat.com/simple-node-js-and-slack-webhook-integration-d87c95aa9600
