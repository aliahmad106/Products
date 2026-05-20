interface SkeletonProps {
  width?: string;
  height?: string;
  borderRadius?: string;
  className?: string;
}

export function Skeleton({ width = '100%', height = '1rem', borderRadius = '4px', className = '' }: SkeletonProps) {
  return (
    <div
      className={`skeleton ${className}`}
      style={{ width, height, borderRadius }}
      aria-hidden="true"
    />
  );
}

export function TableSkeleton({ rows = 5 }: { rows?: number }) {
  return (
    <div className="table-skeleton" aria-label="Loading products">
      {Array.from({ length: rows }).map((_, i) => (
        <div key={i} className="table-skeleton-row">
          <Skeleton width="25%" height="0.875rem" />
          <Skeleton width="35%" height="0.875rem" />
          <Skeleton width="10%" height="0.875rem" />
          <Skeleton width="12%" height="0.875rem" />
          <Skeleton width="15%" height="0.875rem" />
        </div>
      ))}
    </div>
  );
}

export function CardSkeleton({ count = 3 }: { count?: number }) {
  return (
    <div className="card-skeleton-grid" aria-label="Loading products">
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="card-skeleton">
          <Skeleton width="60%" height="1rem" />
          <Skeleton width="40%" height="0.875rem" />
          <Skeleton width="80%" height="0.75rem" />
          <Skeleton width="30%" height="1.25rem" />
        </div>
      ))}
    </div>
  );
}
