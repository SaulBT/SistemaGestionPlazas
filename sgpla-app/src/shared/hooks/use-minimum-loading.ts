import { useEffect, useRef, useState } from "react";

export function useMinimumLoading(
  isLoading: boolean,
  minimumDurationMs = 300,
): boolean {
  const [showLoading, setShowLoading] = useState(isLoading);
  const loadingStartedAt = useRef<number | null>(null);

  useEffect(() => {
    if (isLoading) {
      loadingStartedAt.current ??= Date.now();
      window.setTimeout(() => setShowLoading(true), 0);
      return;
    }

    if (loadingStartedAt.current === null) {
      return;
    }

    const elapsedMs = Date.now() - loadingStartedAt.current;
    const remainingMs = Math.max(0, minimumDurationMs - elapsedMs);
    const timeoutId = window.setTimeout(() => {
      loadingStartedAt.current = null;
      setShowLoading(false);
    }, remainingMs);

    return () => window.clearTimeout(timeoutId);
  }, [isLoading, minimumDurationMs]);

  return isLoading || showLoading;
}
