import axios from "axios";
import { OzonProductDto, TaobaoItemDto } from "../ozonScanner/ozonScanner";

const api = axios.create({ baseURL: "http://localhost:5050", adapter: "fetch" });

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (message.action === "SAVE_PRICE") {
    console.log("Price from", sender.tab?.url, message);
    const body: OzonProductDto = {
      sku: message.sku,
      title: message.title,
      price: message.price,
      originalPrice: message.originalPrice,
      image: message.image,
      availability: message.availability,
      url: message.url,
    };
    api
      .post<TaobaoItemDto[]>("/api/items/", body)
      .then((response) => {
        sendResponse({ ok: true, items: response.data });
      })
      .catch((error) => {
        console.error("API Error:", error);
        sendResponse({ ok: false, error: String(error) });
      });
    return true;
  }
});
