import { useEffect, useRef, useState } from "react";
import { MINIMUM_LOADING_DURATION_MS } from "@/shared/constants/loading";

export function useMinimumLoading(
  isLoading: boolean,
  minimumDurationMs = MINIMUM_LOADING_DURATION_MS,
  delayMs = 150,
): boolean {
  const [showLoading, setShowLoading] = useState(isLoading && delayMs === 0);
  const loadingStartedAt = useRef<number | null>(null);
  const loadingVisibleAt = useRef<number | null>(null);

  useEffect(() => {
    if (isLoading) {
      loadingStartedAt.current ??= Date.now();

      if (showLoading) {
        loadingVisibleAt.current ??= Date.now();
        return;
      }

      const timeoutId = window.setTimeout(() => {
        loadingVisibleAt.current = Date.now();
        setShowLoading(true);
      }, delayMs);

      return () => window.clearTimeout(timeoutId);
    }

    if (loadingStartedAt.current === null) {
      return;
    }

    if (!showLoading || loadingVisibleAt.current === null) {
      loadingStartedAt.current = null;
      return;
    }

    const elapsedMs = Date.now() - loadingVisibleAt.current;
    const remainingMs = Math.max(0, minimumDurationMs - elapsedMs);
    const timeoutId = window.setTimeout(() => {
      loadingStartedAt.current = null;
      loadingVisibleAt.current = null;
      setShowLoading(false);
    }, remainingMs);

    return () => window.clearTimeout(timeoutId);
  }, [delayMs, isLoading, minimumDurationMs, showLoading]);

  return delayMs === 0 ? isLoading || showLoading : showLoading;
}
