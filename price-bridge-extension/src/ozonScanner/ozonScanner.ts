export interface OzonProductDto {
  sku: string;
  title?: string;
  brand?: string;
  category?: string[];
  characteristics?: Record<string, string>;
  price?: number;
  originalPrice?: number;
  images?: string[];
  availability?: string;
  url: string;
}

export interface TaobaoItemDto {
  itemId?: string;
  title?: string;
  translatedTitle?: string;
  price?: number;
  currency: string;
  image?: string;
  url: string;
}

const MAX_IMAGES = 3;

// Widget ids end with a numeric suffix that differs between pages, so match by prefix
function readState(widget: string): any {
  const raw = document
    .querySelector(`div[id^="state-${widget}-"]`)
    ?.getAttribute("data-state");
  if (!raw) return undefined;

  try {
    return JSON.parse(raw);
  } catch {
    return undefined;
  }
}

const toNumber = (s?: string) =>
  s ? Number(s.replace(/[^\d]/g, "")) || undefined : undefined;

// Gallery holds full-size originals; the c600 variant is plenty for image search
const toMediumImage = (src: string) =>
  src.replace(/\/(multimedia-[^/]+)\/(\d+\.\w+)$/, "/$1/c600/$2");

function getBrand(): string | undefined {
  return readState("webBrand")?.content?.title?.text?.[0]?.content;
}

function getCategory(brand?: string): string[] | undefined {
  const crumbs: string[] =
    readState("breadCrumbs")?.breadcrumbs?.map((c: any) => c.text) ?? [];

  // The last crumb is often the brand inside the category, which isn't a category itself
  if (brand && crumbs.at(-1)?.toLowerCase() === brand.toLowerCase())
    crumbs.pop();

  return crumbs.length ? crumbs : undefined;
}

function getCharacteristics(): Record<string, string> | undefined {
  const entries = (readState("webShortCharacteristics")?.characteristics ?? [])
    .map((c: any) => [
      c.title?.textRs?.map((t: any) => t.content).join("") ?? "",
      c.values?.map((v: any) => v.text).join(", ") ?? "",
    ])
    .filter(([key, value]: string[]) => key && value);

  return entries.length ? Object.fromEntries(entries) : undefined;
}

function getImages(fallback?: string): string[] | undefined {
  const gallery: string[] =
    readState("webGallery")?.images?.map((i: any) => i.src) ?? [];
  const images = gallery.length
    ? gallery.slice(0, MAX_IMAGES).map(toMediumImage)
    : [
        document
          .querySelector('meta[property="og:image"]')
          ?.getAttribute("content") || fallback,
      ].filter((i): i is string => !!i);

  return images.length ? images : undefined;
}

export function getOzonProduct(): OzonProductDto | null {
  const variants: any[] =
    readState("webAspects")?.aspects?.flatMap((a: any) => a.variants ?? []) ??
    [];
  const v = variants.find((v) => v.active);
  if (!v) return null;

  const priceState = readState("webPrice");
  const brand = getBrand();

  return {
    sku: v.sku,
    title: readState("webProductHeading")?.title ?? v.data?.title,
    brand,
    category: getCategory(brand),
    characteristics: getCharacteristics(),
    // webPrice.price is the regular price; the variant price is the Ozon Bank card price
    price: toNumber(priceState?.price) ?? v.price ?? toNumber(v.data?.price),
    originalPrice: toNumber(priceState?.originalPrice ?? v.data?.originalPrice),
    images: getImages(v.data?.coverImage),
    availability: v.availability,
    url: window.location.href,
  };
}
