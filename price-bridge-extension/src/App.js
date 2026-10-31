import { useState } from "react";
import "./App.css";

function App() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const getPrice = async () => {
    setLoading(true);
    setError(null);
    setItems([]);

    try {
      const [tab] = await chrome.tabs.query({
        active: true,
        currentWindow: true,
      });

      const response = await chrome.tabs.sendMessage(tab.id, {
        action: "GET_PRICE",
      });

      if (response?.ok) {
        setItems(response.items ?? []);
      } else {
        setError(response?.error ?? "No response");
      }
    } catch (e) {
      setError(String(e));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="App">
      <header className="header">
        <h1>
          Price<span>Bridge</span>
        </h1>
      </header>

      <button className="primary-button" onClick={getPrice} disabled={loading}>
        {loading ? "Searching..." : "Get Price"}
      </button>

      {error && <p className="error">{error}</p>}

      {!loading && !error && items.length === 0 && (
        <p className="empty">No results yet</p>
      )}

      {loading && (
        <div className="items" aria-hidden="true">
          {[0, 1, 2].map((i) => (
            <div key={i} className="skeleton-row">
              <div className="skeleton skeleton-image" />
              <div className="skeleton-lines">
                <div className="skeleton skeleton-line" />
                <div className="skeleton skeleton-line short" />
              </div>
              <div className="skeleton skeleton-price" />
            </div>
          ))}
        </div>
      )}

      <ul className="items">
        {items.map((item) => (
          <li key={item.url}>
            <a href={item.url} target="_blank" rel="noreferrer">
              {item.image && <img src={item.image} alt="" />}
              <span className="title">{item.title}</span>
              {item.price != null && (
                <span className="price">
                  {item.price} {item.currency}
                </span>
              )}
              <span className="chevron">›</span>
            </a>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default App;
