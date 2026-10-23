import { availabilityLabel, formatRating, isFixture } from './catalog-display';

describe('formatRating', () => {
  it('shows the value, the scale and the source', () => {
    expect(formatRating({ value: 7.8, scale: 10, source: 'Fixture rating' })).toEqual({
      text: '7.8/10',
      ariaLabel: 'Rated 7.8 out of 10 by Fixture rating',
    });
  });

  it('shows integer scales without decimals', () => {
    expect(formatRating({ value: 71.4, scale: 100, source: 'Fixture rating' })?.text).toBe('71/100');
  });

  it('returns null when there is no rating', () => {
    expect(formatRating(null)).toBeNull();
  });
});

describe('availabilityLabel', () => {
  it('gives three distinct texts for the three states', () => {
    const texts = new Set([
      availabilityLabel({ status: 'available', region: 'US', providerIds: [] }).text,
      availabilityLabel({ status: 'unavailable', region: 'US' }).text,
      availabilityLabel({ status: 'unknown', region: 'US' }).text,
    ]);
    expect(texts.size).toBe(3);
  });

  it('never labels an unknown state as unavailable', () => {
    expect(availabilityLabel({ status: 'unknown', region: 'US' })).toEqual({
      text: 'Availability unknown',
      tone: 'neutral',
    });
  });

  it('includes the region for known states', () => {
    expect(availabilityLabel({ status: 'available', region: 'GB', providerIds: [] }).text).toBe('Available in GB');
    expect(availabilityLabel({ status: 'unavailable', region: 'GB' }).text).toBe('Not available in GB');
  });
});

describe('isFixture', () => {
  it('is true only for fixture origins', () => {
    expect(isFixture({ kind: 'fixture', label: 'Sample' })).toBe(true);
    expect(isFixture({ kind: 'live', adapter: 'api' })).toBe(false);
  });
});
