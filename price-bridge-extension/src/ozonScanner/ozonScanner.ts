export interface OzonProductDto {
  sku: string;
  title?: string;
  price?: number;
  originalPrice?: number;
  image?: string;
  availability?: string;
  url: string;
}

export interface TaobaoItemDto {
  itemId?: string;
  title?: string;
  price?: number;
  currency: string;
  image?: string;
  url: string;
}

export function getOzonProduct(): OzonProductDto | null {
  const raw = document
    .querySelector('div[id^="state-webAspects-"]')
    ?.getAttribute("data-state");
  if (!raw) return null;

  try {
    const variants: any[] =
      JSON.parse(raw).aspects?.flatMap((a: any) => a.variants ?? []) ?? [];
    const v = variants.find(v => v.active);
    if (!v) return null;

    const toNumber = (s?: string) =>
      s ? Number(s.replace(/[^\d]/g, "")) || undefined : undefined;

    return {
      sku: v.sku,
      title: v.data?.title,
      price: v.price ?? toNumber(v.data?.price),
      originalPrice: toNumber(v.data?.originalPrice),
      image:
        document
          .querySelector('meta[property="og:image"]')
          ?.getAttribute("content") || v.data?.coverImage,
      availability: v.availability,
      url: window.location.href,
    };
  } catch {
    return null;
  }
}