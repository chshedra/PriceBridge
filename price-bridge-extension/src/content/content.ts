import { getOzonProduct } from "../ozonScanner/ozonScanner";

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (message.action === "GET_PRICE") {
    const product = getOzonProduct();
    if (product) {
      chrome.runtime.sendMessage({ action: "SAVE_PRICE", ...product });
    }
  }
});


