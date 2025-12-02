// pages/HomePage.tsx
import HeroBannerCarousel from '../components/HeroBanner';
import RecentlyViewed from '../components/RecentlyViewed';
import { FlashSaleBanner } from '../components/banners/FlashSaleBanner';
import { CommitmentBanner } from '../components/banners/CommitmentBanner';
import BestSellerProducts  from '../components/BestSellerProducts';
import PopularProducts from '../components/PopularProducts';


export default function HomePage() {
  return (
    <>
      <HeroBannerCarousel />

      <FlashSaleBanner />
      <RecentlyViewed />
      {/* <AudioBanner /> */}
      <BestSellerProducts />
      {/* <FastChargeBanner /> */}
      <PopularProducts />
      <CommitmentBanner />

    </>
  );
}