import { Availability, DataOrigin, Rating } from './catalog.models';

/**
 * Callers must show the rating's source next to the value. The text always includes the scale.
 * Integer scales (such as 100) show no decimals. Other values keep up to one decimal place.
 */
export function formatRating(r: Rating | null): { text: string; ariaLabel: string } | null {
  if (!r) return null;
  const value = r.scale >= 100 ? Math.round(r.value).toString() : Number(r.value.toFixed(1)).toString();
  return {
    text: `${value}/${r.scale}`,
    ariaLabel: `Rated ${value} out of ${r.scale} by ${r.source}`,
  };
}

/**
 * The three availability states have distinct wording.
 * 'unknown' is never shown as 'unavailable'.
 */
export function availabilityLabel(a: Availability): { text: string; tone: 'positive' | 'negative' | 'neutral' } {
  switch (a.status) {
    case 'available':
      return { text: `Available in ${a.region}`, tone: 'positive' };
    case 'unavailable':
      return { text: `Not available in ${a.region}`, tone: 'negative' };
    case 'unknown':
      return { text: 'Availability unknown', tone: 'neutral' };
  }
}

export function isFixture(origin: DataOrigin): boolean {
  return origin.kind === 'fixture';
}
