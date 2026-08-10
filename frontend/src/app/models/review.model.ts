// frontend/src/app/models/review.model.ts
export interface Review {
  id: string;
  rating: number;
  comment?: string;
  containsSpoilers: boolean;
  createdAt: Date;
  updatedAt?: Date;
  helpfulCount: number;
  user: ReviewUser;
}

export interface ReviewUser {
  id: string;
  username: string;
  avatarUrl?: string;
}

export interface ReviewSummary {
  totalReviews: number;
  averageRating: number;
  ratingDistribution: { [key: number]: number };
}

export interface CreateReviewRequest {
  rating: number;
  comment?: string;
  containsSpoilers: boolean;
  movieId: string;
}

export interface UpdateReviewRequest {
  rating?: number;
  comment?: string;
  containsSpoilers?: boolean;
}