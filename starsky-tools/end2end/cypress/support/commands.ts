// Get current environment folder
export const envFolder = Cypress.env().configFolder
  ? Cypress.env().configFolder
  : "starsky";

// Get current environment name
export const envName = Cypress.env().name;

// checkStatusCode
declare global {
  // eslint-disable-next-line @typescript-eslint/no-namespace
  namespace Cypress {
    interface Chainable {
      checkStatusCode: typeof checkStatusCode;
    }
  }
}

function checkStatusCode(url: string, statusCode: number[] = [200]) {
  cy.request({
    method: "GET",
    url: url,
    failOnStatusCode: false,
  }).then((res) => {
    expect(res.status).to.oneOf(statusCode);
  });
}

// Verify if page returns statuscode 200, otherwise skip test cases
Cypress.Commands.add("checkStatusCode", checkStatusCode);

// resetStorage
declare global {
  // eslint-disable-next-line @typescript-eslint/no-namespace
  namespace Cypress {
    interface Chainable {
      resetStorage: typeof resetStorage;
    }
  }
}

export function resetStorage() {
  localStorage.clear();
  sessionStorage.clear();
  cy.clearLocalStorage();
}

// Resets all storage
Cypress.Commands.add("resetStorage", resetStorage);

// Send Auth Header as cookie
declare global {
  // eslint-disable-next-line @typescript-eslint/no-namespace
  namespace Cypress {
    interface Chainable {
      sendAuthenticationHeader: typeof sendAuthenticationHeader;
    }
  }
}

function sendAuthenticationHeader() {
  cy.request({
    method: "GET",
    url: Cypress.config().baseUrl + "/account/login",
    form: false,
    followRedirect: false,
    failOnStatusCode: false,
  }).then((response) => {
    if (!response?.headers) {
      console.log("SKIP DUE -- response?.headers is null");      
      return;
    }
    
    const cookies = response.headers["set-cookie"];
    let afCookie = "";
    for (const cookie of cookies) {
      if (cookie.startsWith("X-XSRF-TOKEN")) {
        afCookie = cookie.split(";")[0].replace("X-XSRF-TOKEN=", "");
      }
    }

    cy.request({
      method: "POST",
      url: Cypress.config().baseUrl + "/starsky/api/account/login",
      form: false,
      followRedirect: false,
      headers: {
        "X-XSRF-TOKEN": afCookie,
        "Content-Type": "application/x-www-form-urlencoded",
      },
      body: `Email=${Cypress.env("AUTH_USER")}&Password=${Cypress.env(
        "AUTH_PASS"
      )}`,
      failOnStatusCode: false,
    }).then(() => {
      // expect(res.status).to.eq(200)
    });
  });
}

Cypress.Commands.add("sendAuthenticationHeader", sendAuthenticationHeader);

// upload
declare global {
  // eslint-disable-next-line @typescript-eslint/no-namespace
  namespace Cypress {
    interface Chainable {
      uploadFile: typeof uploadFile;
    }
  }
}

function uploadFile(fileName: string, fileType: string, selector: string) {
  cy.get(selector).then((subject) => {
    console.log("---");

    cy.fixture(fileName, "hex").then((fileHex) => {
      const fileBytes = hexStringToByte(fileHex);
      const testFile = new File([fileBytes], fileName, {
        type: fileType,
      });
      const dataTransfer = new DataTransfer();
      const el = subject[0] as HTMLInputElement;
      dataTransfer.items.add(testFile);
      el.files = dataTransfer.files;

      cy.get(selector).trigger("change", {
        force: true,
      });
    });
  });
}

/**
 * @see: https://stackoverflow.com/a/50338363/8613589
 * @param str string
 */
function hexStringToByte(str) {
  if (!str) {
    return new Uint8Array();
  }
  const a = [];
  for (let i = 0, len = str.length; i < len; i += 2) {
    a.push(parseInt(str.substr(i, 2), 16));
  }
  return new Uint8Array(a);
}

Cypress.Commands.add("uploadFile", uploadFile);

// // API upload
declare global {
  // eslint-disable-next-line @typescript-eslint/no-namespace
  namespace Cypress {
    interface Chainable {
      fileRequest: typeof fileRequest;
    }
  }
}

function fileRequest(fileName: string, to: string, imageType: string) {
  cy.fixture(fileName, "binary").then((imageBin) => {
    const blob = Cypress.Blob.binaryStringToBlob(imageBin, imageType);
    const xhr = new XMLHttpRequest();
    xhr.withCredentials = true;
    const data = new FormData();
    data.set("data", blob, fileName);

    xhr.open("POST", "/api/upload");
    xhr.setRequestHeader("accept", "application/json");
    xhr.setRequestHeader("to", to);
    xhr.onload = function () {
      // done(xhr)
    };
    xhr.onerror = function () {
      // done(xhr)
    };
    xhr.send(data);
  });
}

Cypress.Commands.add("fileRequest", fileRequest);
