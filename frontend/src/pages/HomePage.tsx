// ==================== pages/HomePage.tsx ====================
import React from 'react';
import HeaderAndNavbar from '../components/HeaderAndNavbar';
import HeroBannerCarousel from '../components/HeroBanner';
import ProductSectionDemo from '../components/ProductSection';
import Footer from '../components/Footer';
import { PageType } from '../types';

interface HomePageProps {
  onNavigate: (page: PageType) => void;
}

export default function HomePage({ onNavigate }: HomePageProps) {
  return (
    <>
    <HeaderAndNavbar onNavigate={onNavigate} />
    <HeroBannerCarousel/>
    <ProductSectionDemo/>
    <Footer/>
    </>
  );
}
