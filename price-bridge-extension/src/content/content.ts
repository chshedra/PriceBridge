import { getOzonProduct } from "../ozonScanner/ozonScanner";

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (message.action === "GET_PRICE") {
    const product = getOzonProduct();
    if (!product) {
      sendResponse({ ok: false, error: "Product not found on this page" });
      return;
    }
    chrome.runtime.sendMessage({ action: "SAVE_PRICE", ...product }, sendResponse);
    return true;
  }
});
